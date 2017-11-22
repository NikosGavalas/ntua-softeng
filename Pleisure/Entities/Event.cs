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

		[DBColumn("location_lat")]
		public double Latitude;

		[DBColumn("location_lng")]
		public double Longitude;

		[DBColumn("address")]
		public string Address;

		[DBColumn("duration")]
		public int Duration;

		[DBReference("organizer_id", "user_id")]
		public User Organizer;


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
	}
}
