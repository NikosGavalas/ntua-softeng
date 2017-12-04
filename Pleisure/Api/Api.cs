using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;
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
		}

		public async Task Login(HttpRequest req)
		{
			if (!req.HasPOST("email", "password"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string onSuccess = req.POST("on_success", "/");
			string onFail = req.POST("on_failure", "/?loginfail=1");


			User user = await Auth.Authenticate(req.POST("email"), req.POST("password"));

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
			if (!req.HasPOST("email", "password", "password2", "full_name", "role"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}

			string onSuccess = req.POST("on_success", "/");
			string onFail = req.POST("on_failure", "/?registerfail=1");

			string email = req.POST("email");
			string password = req.POST("password");
			string password2 = req.POST("password2");
			string fullName = req.POST("full_name");
			int role;

			if (password != password2 							// Check if passwords match
			    || !int.TryParse(req.POST("role"), out role) 	// Check if the POSTed role is an integer
			    || !(role == (int)UserRole.Parent 
			         || role == (int)UserRole.Organizer)		// Check if the user isn't trying to bamboozle us
			    || !Auth.ValidateEmail(email)					// Check if the email address is valid
			    || await Auth.EmailTaken(email))				// Check if the email is available				
			{
				await req.Redirect(onFail);
				return;
			}

			// All looks good, register...
			long userId = await Auth.RegisterUser(email, password, fullName, role);

			// Let's also set the session so the user stays logged in
			UserSession session = req.Session as UserSession;
			session.UserID = userId;

			await req.Redirect(onSuccess);
		}

		public async Task SignOut(HttpRequest req)
		{
			string redirect = req.POST("redirect") ?? req.GET("redirect", "/");

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

			if (!req.HasGET("address"))
			{
				req.SetStatusCode(HttpStatusCode.BadRequest);
				await req.Close();
				return;
			}
			
			int distance = int.Parse(req.GET("distance", "1000"));
			Location location = await Google.Geocode(req.GET("address"));


			SelectQuery<Event> query = new SelectQuery<Event>();

			// Filter by distance
			query.Where("DISTANCE(lat, lng, @loc_lat, @loc_lng) < @distance");
			query.AddParameter("@loc_lat", location.Latitude);
			query.AddParameter("@loc_lng", location.Longitude);
			query.AddParameter("@distance", distance);

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

			// Filter by id
			if (req.HasGET("id"))
			{
				query.Where("id", req.GET("id"));
			}

			// Perform query and build the response object
			JArray arr = new JArray();
			List<Event> events = await Program.MySql().Execute(query);
			foreach (Event evt in events)
			{
				arr.Add(await evt.SerializeWithScheduled());
			}

			await req.Write(arr.ToString());

			await req.Close();
		}


	}
}
