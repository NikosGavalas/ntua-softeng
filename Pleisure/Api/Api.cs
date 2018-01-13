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
			apiRouter.Add("/kids", Kids);
			apiRouter.Add("/events", Events);
			apiRouter.Add("/email_available", EmailAvailable);

			apiRouter.Add("/api/add_kid", AddKid);
			apiRouter.Add("/api/create_event", CreateEvent);
			apiRouter.Add("/api/schedule_event", ScheduleEvent);
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
			    || !int.TryParse("gender", out gender)
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

			req.SetStatusCode(result.RowsAffected > 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
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

			if (await req.HasPOST("image"))
			{
				MemoryStream imgStream = await req.GetContentData("image");
				string filePath = Options.StoragePath(string.Format("eventimg/{0}.png", eventId));

				byte[] buffer = new byte[imgStream.Length];
				await imgStream.ReadAsync(buffer, 0, buffer.Length);

				FileStream writer = File.OpenRead(filePath);
				await writer.WriteAsync(buffer, 0, buffer.Length);

				req.SetContentTypeByExtension(ContentType.Image, "png");
				req.SetStatusCode(HttpStatusCode.OK);
				await req.Write(buffer);
				await req.Close();
				return;
			}

			if (!await req.HasPOST("title", "description", "price", "duration", "address"))
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

			if (!int.TryParse(await req.POST("price"), out price)
			   || !int.TryParse(await req.POST("duration"), out duration))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			req.SetContentType(ContentType.Json);

			await req.Close();
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
			await req.Close();
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
			}
			else
			{
				JArray response = new JArray();

				foreach (Kid kid in await user.GetKids())
				{
					response.Add(kid.Serialize());
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

			if (location == null || distance > 50000)
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Event>();

			// Filter by distance
			query.Where("DISTANCE(lat, lng, @loc_lat, @loc_lng) < @distance")
				.AddParameter("@loc_lat", location.Latitude)
				.AddParameter("@loc_lng", location.Longitude)
				.AddParameter("@distance", distance);

			// Filter by price
			if (req.HasGET("price"))
			{
				query.Where("price", WhereRelation.UpTo, req.GET("price"));
			}

			// Filter by age
			if (req.HasGET("age"))
			{
				query.Where("age_min", WhereRelation.UpTo, req.GET("age"))
					.Where("age_max", WhereRelation.AtLeast, req.GET("age"));
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

			// Perform query and build the response object
			JArray arr = new JArray();
			List<Event> events = await Program.MySql().Execute(query);
			foreach (Event evt in events)
			{
				DateTime minDate = DateTime.Now;
				DateTime maxDate = DateTime.MaxValue;

				DateTime.TryParse(await req.POST("min_date"), out minDate);
				DateTime.TryParse(await req.POST("max_date"), out maxDate);

				if (await evt.HappensBetween(minDate, maxDate))
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
					Event evt = Event.Random(id, location.Latitude, location.Longitude, distance);
					arr.Add(evt.Serialize());
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


	}
}
