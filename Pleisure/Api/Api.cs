using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;
using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public class Api
	{

		public Api(Router router)
		{
			router.Add("/kids", Kids);
			router.Add("/events", Events);
		}

		public async Task Kids(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);


			await request.Close();
		}

		public async Task Events(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);

			if (request.GET("address") == null)
			{
				request.SetStatusCode(System.Net.HttpStatusCode.BadRequest);
				await request.Close();
				return;
			}
			
			int distance = int.Parse(request.GET("distance", "1000"));
			Location location = await Google.Geocode(request.GET("address"));

			JArray arr = new JArray();

			SelectQuery<Event> query = new SelectQuery<Event>();


			query.Where("DISTANCE(lat, lng, @loc_lat, @loc_lng) < @distance");
			query.AddParameter("@loc_lat", location.Latitude);
			query.AddParameter("@loc_lng", location.Longitude);
			query.AddParameter("@distance", distance);
			
			List<Event> events = await Program.MySql().Execute(query);
			foreach (Event evt in events)
			{
				arr.Add(evt.Serialize());
			}

			await request.Write(arr.ToString());

			await request.Close();
		}


	}
}
