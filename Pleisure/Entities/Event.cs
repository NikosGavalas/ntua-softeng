using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
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
				thumbnail = "http://via.placeholder.com/128x128"
			});

			return obj;
		}

		public async Task<JToken> SerializeWithScheduled(bool includeAttendance)
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
