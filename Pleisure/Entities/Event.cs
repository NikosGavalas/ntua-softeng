using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
using ChanceNET;
using Newtonsoft.Json.Linq;



namespace Pleisure
{
	[DBTable("events")]
	public class Event
	{
		[DBColumn("event_id")]
		public int ID;

		[DBColumn("title")]
		public string Title;

		[DBColumn("description")]
		public string Description;

		[DBColumn("price")]
		public int Price;

		[DBColumn("lat")]
		public double Latitude;

		[DBColumn("lng")]
		public double Longitude;

		[DBColumn("address")]
		public string Address;

		[DBColumn("duration")]
		public int Duration;

		[DBReference("organizer_id", "user_id")]
		public User Organizer;

		[DBColumn("age_min")]
		public int AgeMin;

		[DBColumn("age_max")]
		public int AgeMax;

		public string Thumbnail = "http://via.placeholder.com/128x128";

		public static Event Random(int id, double centerLat, double centerLng, double range)
		{
			Chance c = new Chance(id);
			int ageMin = c.Integer(4, 18);
			ChanceNET.Location loc = c.Location(centerLat, centerLng, range);
			return new Event()
			{
				ID =			id,
				Title =			c.Sentence(capitalize: true),
				Description =	c.Paragraph(),
				Price =			c.Natural(100),
				Latitude =		loc.Latitude,
				Longitude =		loc.Longitude,
				Address =		c.Address(numberFirst: false),
				Duration =		c.PickOne(new int[] { 30, 45, 60, 75, 90, 120, 180 }),
				Thumbnail =		c.Avatar(GravatarDefaults.Identicon),
				AgeMin =		ageMin,
				AgeMax =		c.Integer(ageMin, 18),
				Organizer =		User.Random(c, UserRole.Organizer)
			};
		}

		public JToken Serialize()
		{
			JToken obj = JToken.FromObject(new
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
				thumbnail = Thumbnail
			});

			return obj;
		}

		public async Task<JToken> SerializeWithScheduled(bool includeAttendance = false)
		{
			JToken obj = Serialize();
			obj["scheduled"] = new JArray();
			 
			foreach (ScheduledEvent scheduled in await GetScheduled())
			{
				JToken item = await scheduled.Serialize(includeAttendance);

				obj.Value<JArray>("scheduled").Add(item);
			}

			return obj;
		}

		public Task<List<ScheduledEvent>> GetScheduled()
		{
			SelectQuery<ScheduledEvent> query = new SelectQuery<ScheduledEvent>()
				.Where<SelectQuery<ScheduledEvent>>("event_id", ID);

			return Program.MySql().Execute(query);
		}
	}
}
