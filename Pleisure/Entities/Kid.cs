using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
using Newtonsoft.Json.Linq;

using ChanceNET;

namespace Pleisure
{
	[DBTable("kids")]
	public class Kid
	{
		[DBColumn("kid_id")]
		public int ID;

		[DBColumn("name")]
		public string Name;

		[DBColumn("birthday")]
		public DateTime Birthday;

		[DBColumn("gender")]
		public Gender Gender;

		[DBReference("parent_id", "user_id")]
		public User Parent;

		public int Age
		{
			get
			{
				int age = DateTime.Now.Year - Birthday.Year;
				if (DateTime.Now.DayOfYear < Birthday.DayOfYear && age > 0)
					age--;
				return age;
			}
		}

		public async Task<JToken> Serialize()
		{
			return JToken.FromObject(new
			{
				kid_id = ID,
				name = Name,
				age = Age,
				gender = (int)Gender,
				parent = new
				{
					id = Parent.ID,
					name = Parent.FullName,
					avatar = Parent.Avatar
				},
				avatar = Options.Gravatar(ID.ToString()),
				attending = (await AttendingEvents()).Select(e => 
				{
					JToken evt = e.Serialize(false).Result;
					evt["event"] = e.Event.Serialize();
					return evt;
				})
			});
		}

		public async Task<List<ScheduledEvent>> AttendingEvents()
		{
			SelectQuery<EventAttendance> query = new SelectQuery<EventAttendance>();
			query.Where("kid_id", ID);

			List<EventAttendance> attendances = await Program.MySql().Execute(query);

			if (attendances.Count == 0)
				return new List<ScheduledEvent>();

			SelectQuery<ScheduledEvent> eventsQuery = new SelectQuery<ScheduledEvent>();
			WhereClause clause = null;

			attendances.ForEach(a => 
			{
				clause |= new WhereClause("scheduled_event_id", a.ScheduledID);
			});

			eventsQuery.Where(clause);

			List<ScheduledEvent> events = await Program.MySql().Execute(eventsQuery);

			return events;
		}
	}

	public enum Gender
	{
		Male = 0,
		Female = 1,
		Any = 2
	}
}
