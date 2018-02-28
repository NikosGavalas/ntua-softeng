using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;
using ChanceNET;
using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public class Api
	{

		public Api(WebServer server)
		{
			server.Add("/login", Login);
			server.Add("/register", Register);
			server.Add("/signout", SignOut);

			Router apiRouter = server.AddRouter("/api");
			apiRouter.Add("/kids", Kids)
			         .Add("/user", User)
					 .Add("/events", Events)
					 .Add("/event", Event)
			         .Add("/email_available", EmailAvailable)
			         .Add("/add_kid", AddKid)
			         .Add("/create_event", CreateEvent)
			         .Add("/schedule_event", ScheduleEvent)
					 .Add("/own_events", OwnEvents)
					 .Add("/own_event", OwnEvent)
			         .Add("/book_event", BookEvent)
			         .Add("/categories", Categories)
					 .Add("/user_update", UserUpdate)
			         .Add("/pay", Pay);

			/*
			 * Admin APIs
			 */
			apiRouter.Add("/users", Users)
			         .Add("/ban_user", BanUser);
		}

		public async Task UserUpdate(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;
	
			User user = await session.GetUser();

			string redirectTo = "/profile";

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Unauthorized);
				await req.Close();
				return;
			}

			if (await req.HasPOST("id"))
			{
				if (user.Role != UserRole.Admin)
				{
					await req.SetStatusCode(HttpStatusCode.Forbidden).Close();
					return;
				}

				SelectQuery<User> sel = new SelectQuery<User>();
				sel.Where("user_id", await req.POST("id"));

				user = (await Program.MySql().Execute(sel)).FirstOrDefault();

				if (user == null)
				{
					await req.SetStatusCode(HttpStatusCode.NotFound).Close();
					return;
				}

				redirectTo = "/admin";
			}

			/*
			 * First update the password
			 */
			string password = await req.POST("password", "");
			string password2 = await req.POST("password2", "");
			if (!string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(password2))
			{
				if (!await Auth.UpdatePassword(user, password, password2))
				{
					await req.SetStatusCode(HttpStatusCode.BadRequest).Close();
					return;
				}
			}

			/*
			 * Then update the rest of the data
			 */
			UpdateQuery<User> query = new UpdateQuery<User>();
			query.Where("user_id", user.ID);

			if (await req.HasPOST("email") 
				&& !string.IsNullOrWhiteSpace(await req.POST("email")) 
				&& user.Email != await req.POST("email"))
			{
				if (await Auth.EmailTaken(await req.POST("email")))
				{
					await req.SetStatusCode(HttpStatusCode.Found).Close();
					return;
				}
				query.Set("email", await req.POST("email"));
			}
			if (await req.HasPOST("full_name") && !string.IsNullOrWhiteSpace(await req.POST("full_name")))
			{
				query.Set("full_name", await req.POST("full_name"));
			}
			if (await req.HasPOST("address") && !string.IsNullOrWhiteSpace(await req.POST("address")))
			{
				query.Set("address", await req.POST("address"));
			}

			await Program.MySql().Execute(query);

			await req.Redirect(redirectTo);
		}


		public async Task Event(HttpRequest req)
		{

			int eventId = -1;
			if (!req.HasGET("event_id") || !int.TryParse(req.GET("event_id"), out eventId))
			{
				await req.SetStatusCode(HttpStatusCode.BadRequest).Close();
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Pleisure.Event>();
			query.Where("event_id", eventId);

			Event evt = (await Program.MySql().Execute(query)).FirstOrDefault();

			if (evt == null)
			{
				await req.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			req.SetContentType(ContentType.Json).SetStatusCode(HttpStatusCode.OK);
			await req.Write((await evt.SerializeWithScheduled()).ToString());

			await req.Close();
		}

		public async Task User(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Unauthorized);
				await req.Close();
				return;
			}

			if (user.Role == UserRole.Admin && req.HasGET("user_id"))
			{
				SelectQuery<User> query = new SelectQuery<User>();
				query.Where("user_id", req.GET("user_id"));

				user = (await Program.MySql().Execute(query)).First();
			}

			req.SetStatusCode(HttpStatusCode.OK);
			await req.Write((await user.Serialize()).ToString());

			await req.Close();
		}

		public async Task AddKid(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Unauthorized);
				await req.Close();
				return;
			}

			if (!await req.HasPOST("name", "birthday", "gender"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string name = await req.POST("name");
			DateTime birthday;
			int gender;

			if (!DateTime.TryParse(await req.POST("birthday"), out birthday)
			    || !int.TryParse(await req.POST("gender"), out gender)
			   || (gender != 0 && gender != 1))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			InsertQuery query = new InsertQuery("kids");
			query.Value("name", name)
			     .Value("birthday", birthday)
			     .Value("gender", gender)
			     .Value("parent_id", user.ID);

			NonQueryResult result = await Program.MySql().ExecuteNonQuery(query);

			string url = await req.POST("redirect", "/profile");

			await req.Redirect(url);
		}

		public async Task Categories(HttpRequest req)
		{
			List<Category> categories = await Program.MySql().Select<Category>();

			JArray response = JArray.FromObject(categories.Select(c => c.Serialize()));

			req.SetStatusCode(HttpStatusCode.OK)
			   .SetContentType(ContentType.Json);
			await req.Write(response.ToString());

			await req.Close();
		}

		public async Task CreateEvent(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Unauthorized);
				await req.Close();
				return;
			}

			int eventId = Program.Chance().Natural();

			if (!await req.HasPOST("title", "description", "price", "duration", "address",
			                       "datetime", "recurrence"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string title = await req.POST("title");
			string description = await req.POST("description");
			int price;
			int duration;
			string address = await req.POST("address");
			int genders = 2;
			Location location = await Google.Geocode(address);
			int age_min = 4;
			int age_max = 17;


			string scheduledTime = await req.POST("datetime");
			string recurrence = await req.POST("recurrence");


			int.TryParse(await req.POST("genders"), out genders);
			int.TryParse(await req.POST("age_min"), out age_min);
			int.TryParse(await req.POST("age_max"), out age_max);


			if (!int.TryParse(await req.POST("price"), out price)
			   || !int.TryParse(await req.POST("duration"), out duration))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			// First try scheduling
			if (location == null
			    || !await ScheduleEvent(eventId, scheduledTime, recurrence))
			{
				if (location == null)
					Console.WriteLine("Bad event location");
				req.SetStatusCode(HttpStatusCode.NotAcceptable);
				await req.Close();
				return;
			}


			if (await req.HasPOST("image"))
			{
				MemoryStream imgStream = await req.GetContentData("image");

				string directory = Options.StoragePath("eventimg");
				string filePath = string.Format("{0}/{1}.png", directory, eventId);
				Directory.CreateDirectory(directory);

				byte[] buffer = new byte[imgStream.Length];
				await imgStream.ReadAsync(buffer, 0, buffer.Length);

				using (FileStream writer = File.OpenWrite(filePath))
				{
					await writer.WriteAsync(buffer, 0, buffer.Length);
					await writer.FlushAsync();
				}
			}

			InsertQuery query = new InsertQuery("events");
			query.Value("organizer_id", user.ID)
			     .Value("event_id", eventId)
			     .Value("title", title)
			     .Value("description", description)
			     .Value("price", price)
			     .Value("lat", location.Latitude)
			     .Value("lng", location.Longitude)
				 .Value("address", address)
				 .Value("duration", duration)
			     .Value("age_min", age_min)
			     .Value("age_max", age_max)
			     .Value("genders", genders);

			NonQueryResult result = await Program.MySql().ExecuteNonQuery(query);


			int category;
			if (await req.HasPOST("category") && int.TryParse(await req.POST("category"), out category))
			{
				InsertQuery categoryQuery = new InsertQuery("event_categories");
				categoryQuery.Value("event_id", eventId)
							 .Value("category_id", category);

				await Program.MySql().ExecuteNonQuery(categoryQuery);
			}

			await req.Redirect("/event/" + eventId);
		}

		public async Task ScheduleEvent(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Unauthorized);
				await req.Close();
				return;
			}

			int eventId;
			string recurrence = await req.POST("recurrence", "once");

			if (!await req.HasPOST("event_id", "datetime")
			    || !int.TryParse(await req.POST("event_id"), out eventId))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			bool status = await ScheduleEvent(eventId, await req.POST("datetime"), recurrence);

			req.SetStatusCode(status ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

			await req.Redirect("/profile");
		}

		async Task<bool> ScheduleEvent(int eventId, string dateTime, string recurrence)
		{
			DateTime date;
			EventRecurrence rec;

			if (!DateTime.TryParse(dateTime, out date)
				|| !Enum.TryParse(recurrence, true, out rec))
			{
				return false;
			}

			InsertQuery query = new InsertQuery("scheduled_events");
			query.Value("event_id", eventId)
			     .Value("next_time", date)
			     .Value("recurrence", rec.ToString().ToLower());

			NonQueryResult result = await Program.MySql().ExecuteNonQuery(query);

			return result.RowsAffected == 1;
		}

		public async Task Login(HttpRequest req)
		{
			if (!await req.HasPOST("email", "password"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string onSuccess = await req.POST("on_success", "/");
			string onFail = await req.POST("on_failure", "/?loginfail=1");


			User user = await Auth.Authenticate(await req.POST("email"), await req.POST("password"));

			if (user == null)
			{
				// Login failed
				await req.Redirect(onFail);
			}
			else
			{
				// Login was successful
				UserSession session = req.Session as UserSession;

				// Set the session to keep the user logged in
				session.UserID = (int)user.ID;

				await req.Redirect(onSuccess);

				Console.WriteLine("User logged in: " + user.FullName);
			}
		}

		public async Task EmailAvailable(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);
			if (!req.HasGET("email"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string email = req.GET("email");

			bool isValid = Auth.ValidateEmail(email);
			bool isAvailable = isValid && !await Auth.EmailTaken(email);

			JToken response = JToken.FromObject(new
			{
				valid		= isValid,
				available	= isAvailable
			});

			req.SetStatusCode(HttpStatusCode.OK);

			await req.Write(response.ToString());
			await req.Close();
		}

		public async Task Register(HttpRequest req)
		{
			if (!await req.HasPOST("email", "password", "password2", "full_name", "role"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string onSuccess = await req.POST("on_success", "/");
			string onFail = await req.POST("on_failure", "/?registerfail=1");

			string email = await req.POST("email");
			string password = await req.POST("password");
			string password2 = await req.POST("password2");
			string fullName = await req.POST("full_name");
			string address = await req.POST("address");

			int role;

			if (password != password2 							// Check if passwords match
			    || !int.TryParse(await req.POST("role"), out role) 	// Check if the POSTed role is an integer
			    || !(role == (int)UserRole.Parent 
			         || role == (int)UserRole.Organizer)		// Check if the user isn't trying to bamboozle us
			    || !Auth.ValidateEmail(email)					// Check if the email address is valid
			    || await Auth.EmailTaken(email))				// Check if the email is available				
			{
				await req.Redirect(onFail);
				return;
			}

			// All looks good, register...
			long userId = await Auth.RegisterUser(email, password, fullName, role, address);

			// Let's also set the session so the user stays logged in
			UserSession session = req.Session as UserSession;
			session.UserID = userId;

			await req.Redirect(onSuccess);

			Console.WriteLine("User registered: " + fullName);
		}

		public async Task SignOut(HttpRequest req)
		{
			string redirect = await req.POST("redirect") ?? req.GET("redirect", "/");

			req.Session.Destroy();

			await req.Redirect(redirect);
		}

		public async Task Kids(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}
			else
			{
				JArray response = new JArray();

				foreach (Kid kid in await user.GetKids())
				{
					response.Add(await kid.Serialize());
				}

				await req.Write(response.ToString());
			}

			await req.Close();
		}

		public async Task Events(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);
			
			// Check if we have enough parameters for the search
			if (!req.HasGET("address") && !req.HasGET("lat", "lng"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}
			
			int distance = int.Parse(req.GET("distance", "1000"));
			Location location;

			if (req.HasGET("lat", "lng"))
			{
				location = new Location(double.Parse(req.GET("lat")), double.Parse(req.GET("lng")));
			}
			else
			{
				location = await Google.Geocode(req.GET("address"));
			}

			if (location == null || distance > 50)
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Event>();

			// Filter by distance
			query.Where("HAVERSINE(lat, lng, @loc_lat, @loc_lng) < @distance")
				.AddParameter("@loc_lat", location.Latitude)
				.AddParameter("@loc_lng", location.Longitude)
				.AddParameter("@distance", distance * 1000);

			// Filter by price
			if (req.HasGET("price"))
			{
				query.Where("price", WhereRelation.UpTo, req.GET("price"));
			}

			// Filter by age
			int age = 0;
			if (req.HasGET("age") && int.TryParse(req.GET("age"), out age))
			{
				query.Where("age_min", WhereRelation.UpTo, age)
					.Where("age_max", WhereRelation.AtLeast, age);
			}

			// Filter by duration
			if (req.HasGET("duration_min"))
			{
				query.Where("duration", WhereRelation.AtLeast, req.GET("duration_min"));
			}
			if (req.HasGET("duration_max"))
			{
				query.Where("duration", WhereRelation.UpTo, req.GET("duration_max"));
			}

			// Filter by gender
			if (req.HasGET("gender") && (req.GET("gender") == "male" || req.GET("gender") == "female"))
			{
				query.Where(
					new WhereClause("genders", req.GET("gender"))
	                | new WhereClause("genders", 2)
				);
			}

			// Perform query and build the response object
			JArray arr = new JArray();
			List<Event> events = await Program.MySql().Execute(query);

			foreach (Event evt in events)
			{
				DateTime minDate = DateTime.Now;
				DateTime maxDate = DateTime.MaxValue;

				if (req.HasGET("min_date") && !string.IsNullOrWhiteSpace(req.GET("min_date")))
					DateTime.TryParse(req.GET("min_date"), out minDate);

				if (req.HasGET("max_date") && !string.IsNullOrWhiteSpace(req.GET("max_date")))
					DateTime.TryParse(req.GET("max_date"), out maxDate);

				int categoryId = -1;

				if (req.HasGET("category") && !string.IsNullOrWhiteSpace(req.GET("category")))
					int.TryParse(req.GET("category"), out categoryId);
				
				// DateTime.TryParse(req.GET("max_date"), out maxDate);


				// int.TryParse(req.GET("category"), out categoryId);

				if (await evt.HappensBetween(minDate, maxDate)
				    && (categoryId < 0 || await evt.HasCategories(categoryId)))
				{
					arr.Add(await evt.SerializeWithScheduled());
				}
			}

			if (Options.Randomized)
			{
				Chance c = new Chance(); 
				for (int i = 0; i < 50; i++)
				{
					int id = c.Natural();
					Event evt = Pleisure.Event.Random(id, location.Latitude, location.Longitude, distance);
					arr.Add(await evt.SerializeWithScheduled());
				}
			}

			JToken response = JToken.FromObject(new
			{
				center = location.Serialize(),
				results = arr
			});

			await req.Write(response.ToString());

			await req.Close();
		}

		public async Task OwnEvents(HttpRequest req)
		{
			req.SetContentType(ContentType.Json);

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null || user.Role != UserRole.Organizer)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Event>();
			query.Where("organizer_id", user.ID);

			JArray arr = new JArray();
			List<Event> events = await Program.MySql().Execute(query);
			foreach (Event evt in events)
			{
				arr.Add(await evt.SerializeWithScheduled(true));
			}

			await req.Write(arr.ToString());

			await req.Close();
		}

		public async Task OwnEvent(HttpRequest req)
		{

			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null || user.Role != UserRole.Organizer)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}

			if (!req.HasGET("event_id"))
			{
				await req.SetStatusCode(HttpStatusCode.BadRequest).Close();
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Event>();
			query.Where("organizer_id", user.ID)
			     .Where("event_id", req.GET("event_id"));

			Event evt = (await Program.MySql().Execute(query)).FirstOrDefault();

			if (evt == null)
			{
				await req.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			req.SetContentType(ContentType.Json)
			   .SetStatusCode(HttpStatusCode.OK);

			await req.Write((await evt.SerializeWithScheduled(true)).ToString());

			await req.Close();
		}

		public async Task BookEvent(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}


			if (!await req.HasPOST("scheduled_id", "kid_id"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}


			SelectQuery<ScheduledEvent> scheduledQuery = new SelectQuery<ScheduledEvent>();
			scheduledQuery.Where("scheduled_event_id", await req.POST("scheduled_id"));

			SelectQuery<Kid> kidQuery = new SelectQuery<Kid>();
			kidQuery.Where("kid_id", await req.POST("kid_id"));


			ScheduledEvent scheduled = (await Program.MySql().Execute(scheduledQuery)).FirstOrDefault();
			Kid kid = (await Program.MySql().Execute(kidQuery)).FirstOrDefault();

			if (scheduled == null || kid == null)
			{
				await req.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			Event evt = scheduled.Event;

			if (user.Credits < evt.Price)
			{
				await req.SetStatusCode(HttpStatusCode.PaymentRequired).Close();
				return;				
			}

			if (evt.Organizer.Role != UserRole.Organizer
				|| kid.Age > evt.AgeMax || kid.Age < evt.AgeMin
			    || !(evt.Genders == kid.Gender || (int)evt.Genders > 1))
			{
				await req.SetStatusCode(HttpStatusCode.ExpectationFailed).Close();
				return;
			}

			SelectQuery<EventAttendance> existingAttendance = new SelectQuery<EventAttendance>();
			existingAttendance.Where("scheduled_id", scheduled.ID)
			                  .Where("kid_id", kid.ID);

			bool success = await Auth.BookEvent(user, kid, scheduled);

			await req.Redirect("/profile");

			BookingEmail email = new BookingEmail(user);
			await email.Body(scheduled, kid, evt.Price);
			await email.Send();
		}

		public async Task Users(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null || user.Role != UserRole.Admin)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}

			SelectQuery<User> query = new SelectQuery<User>();


			int role;
			if (req.HasGET("role") && int.TryParse(req.GET("role"), out role))
			{
				query.Where("role", role);
			}
			

			List<User> users = await Program.MySql().Execute(query);

			JArray response = new JArray();

			foreach (User u in users)
			{
				response.Add(await u.Serialize());
			}


			await req.SetContentType(ContentType.Json)
				.SetStatusCode(HttpStatusCode.OK)
				.Write(response.ToString());

			await req.Close();
		}


		public async Task BanUser(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null || user.Role != UserRole.Admin)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}

			if (!await req.HasPOST("user_id"))
			{
				await req.SetStatusCode(HttpStatusCode.BadRequest).Close();
				return;
			}

			SelectQuery<User> query = new SelectQuery<User>();
			query.Where("user_id", await req.POST("user_id"));


			User userToBan = (await Program.MySql().Execute(query)).FirstOrDefault();

			if (userToBan == null)
			{
				await req.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			await Auth.BanUser(userToBan);

			await req.SetStatusCode(HttpStatusCode.OK).Close();
		}

		public async Task Pay(HttpRequest req)
		{
			UserSession session = req.Session as UserSession;

			User user = await session.GetUser();

			if (user == null)
			{
				req.SetStatusCode(HttpStatusCode.Forbidden);
				await req.Close();
				return;
			}

			int amount;
			if (!await req.HasPOST("cc_num", "cc_name", "cc_exp_month", "cc_exp_year", "cvv", "amount")
			   || !int.TryParse(await req.POST("amount"), out amount))
			{
				await req.SetStatusCode(HttpStatusCode.BadRequest)
				         .Close();
				return;
			}

			Console.WriteLine("Processing payment of ${0} for user {1}", amount, user.FullName);

			if (!await Auth.VerifyPayment(await req.POST("cc_num"),
										  await req.POST("cc_name"),
										  await req.POST("cc_exp"),
			                              await req.POST("cvv"),
			                             amount))
			{
				await req.SetStatusCode(HttpStatusCode.PaymentRequired)
				         .Close();
				return;
			}

			/*
			 * Payment verified, give the user his credits
			 */
			Console.WriteLine("Credits before: " + user.Credits);
			await Auth.AddCredits(user, amount);
			Console.WriteLine("Credits after: " + user.Credits);
			User after = Program.MySql().Select<User>().Result.Where(u => u.ID == user.ID).First();
			Console.WriteLine("Credits after: " + after.Credits);

			await req.SetStatusCode(HttpStatusCode.OK)
			         .Close();
		}
	}
}
