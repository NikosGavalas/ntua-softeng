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

		[DBColumn("genders")]
		public Gender Genders;

		public string Thumbnail 
		{
			get { return "/eventthumb/" + ID; }
		}

		public static Event Random(int id, double centerLat, double centerLng, double range)
		{
			Chance c = new Chance(id);
			int ageMin = c.Integer(4, 18);
			ChanceNET.Location loc = c.Location(centerLat, centerLng, range);
			return new Event()
			{
				ID =			id,
				Title =			c.Sentence(words: 6, capitalize: true),
				Description =	c.Paragraph(),
				Price =			c.Natural(100),
				Latitude =		loc.Latitude,
				Longitude =		loc.Longitude,
				Address =		c.Address(numberFirst: false),
				Duration =		c.PickOne(new int[] { 30, 45, 60, 75, 90, 120, 180 }),
				AgeMin =		ageMin,
				AgeMax =		c.Integer(ageMin, 18),
				Organizer =		User.Random(c, UserRole.Organizer),
				Genders	=		c.PickEnum<Gender>()
			};
		}

		public async Task<JToken> Serialize()
		{
			List<Category> categories = await Categories();
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
				thumbnail = Thumbnail,
				gender = Genders.ToString().ToLower(),
				categories = categories.Select(c => c.Serialize())
			});

			return obj;
		}

		public async Task<JToken> SerializeWithScheduled(bool includeAttendance = false)
		{
			JToken obj = await Serialize();
			obj["scheduled"] = new JArray();
			 
			foreach (ScheduledEvent scheduled in await GetScheduled())
			{
				JToken item = await scheduled.Serialize(includeAttendance);

				obj.Value<JArray>("scheduled").Add(item);
			}

			return obj;
		}

		public async Task<List<ScheduledEvent>> GetScheduled()
		{
			SelectQuery<ScheduledEvent> query = new SelectQuery<ScheduledEvent>()
				.Where<SelectQuery<ScheduledEvent>>("event_id", ID);

			List<ScheduledEvent> scheduled = await Program.MySql().Execute(query);

			if (Options.Randomized)
			{
				Chance c = new Chance(ID);
				scheduled.Add(ScheduledEvent.Random(c, this));
			}

			return scheduled;
		}

		public async Task<bool> HappensBetween(DateTime fromDate, DateTime toDate)
		{
			foreach (ScheduledEvent scheduled in await GetScheduled())
			{
				if (scheduled.NextTime >= fromDate && scheduled.NextTime <= toDate)
				{
					return true;
				}
			}
			return false;
		}

		public async Task<List<Category>> Categories()
		{
			Query matchQuery = new Query("SELECT category_id FROM event_categories WHERE event_id=@eid");
			matchQuery.AddParameter("@eid", ID);

			SelectQuery<Category> categoriesQuery = new SelectQuery<Category>();
			WhereClause clause = new WhereClause("1 = 2");

			ResultSet matches = await Program.MySql().Execute(matchQuery);
			foreach (ResultRow match in matches)
			{
				clause |= new WhereClause("category_id", match.GetInteger("category_id"));
			}
			categoriesQuery.Where(clause);

			return await Program.MySql().Execute(categoriesQuery);
		}

		public async Task<bool> HasCategories(params int[] categoryIds)
		{
			List<Category> categories = await Categories();
			foreach (int id in categoryIds)
			{
				if (!categories.Any(c => c.ID == id))
					return false;
			}
			return true;
		}
	}
}
