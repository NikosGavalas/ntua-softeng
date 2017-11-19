using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public class Event
	{
		public int ID { get; private set; }
		public User Organizer { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public int Price { get; private set; }
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }
		public string Address { get; private set; }
		public int Duration { get; private set; }


		public JToken Serialize()
		{
			return JToken.FromObject(new
			{
				id = ID,
				title = Title,
				price = Price,
				coordinates = new
				{
					lat = Latitude,
					lng = Longitude,
				},
				description = Description,
				duration = Duration,
				address = Address,
				thumbnail = "http://via.placeholder.com/128x128"
			});
		}


		public static async Task<Event> WithId(int eventId)
		{
			List<Event> events = new List<Event>();

			HaathMySql conn = Program.Mysql();
			Query query = new Query("SELECT * FROM events WHERE event_id=@eid");
			query.AddParameter("@eid", eventId);

			DBTable result = await conn.Execute(query);

			if (result.RowCount == 0)
				return null;

			DBRow res = result.First();

			return new Event()
			{
				ID = res.GetInteger("event_id"),
				Organizer = await User.WithId(res.GetInteger("organizer_id")),
				Title = res.GetString("title"),
				Description = res.GetString("description"),
				Price = res.GetInteger("price"),
				Latitude = res.GetDouble("location_lat"),
				Longitude = res.GetDouble("location_long"),
				Address = res.GetString("address"),
				Duration = res.GetInteger("duration")
			};
		}

		public static async Task<List<Event>> WithFilters()
		{
			List<Event> events = new List<Event>();

			HaathMySql conn = Program.Mysql();
			Query query = new Query("SELECT * FROM events");

			query.OnRow += async (s, row) =>
			{
				events.Add(new Event()
				{
					ID = row.GetInteger("event_id"),
					Organizer = await User.WithId(row.GetInteger("organizer_id")),
					Title = row.GetString("title"),
					Description = row.GetString("description"),
					Price = row.GetInteger("price"),
					Latitude = row.GetDouble("location_lat"),
					Longitude = row.GetDouble("location_long"),
					Address = row.GetString("address"),
					Duration = row.GetInteger("duration")
				});
			};

			await conn.Execute(query);

			return events;
		}
	}
}
