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
	[DBTable("scheduled_events")]
	public class ScheduledEvent
	{
		[DBColumn("scheduled_event_id")]
		public int ID;

		[DBColumn("next_time")]
		public DateTime NextTime;

		public EventRecurrence Recurrence;

		[DBReference("event_id", "event_id")]
		public Event Event;
		
		public static ScheduledEvent Random(Chance chance, Event evt)
		{
			DateTime n = DateTime.Now;

			return new ScheduledEvent()
			{
				ID			= chance.Natural(),
				NextTime	= new DateTime(n.Year, n.Month, chance.Integer(n.Day, DateTime.DaysInMonth(n.Year, n.Month)),
										   chance.Hour(true), chance.Minute(), 0),
				Recurrence	= chance.PickEnum<EventRecurrence>(),
				Event 		= evt
			};
		}

		public async Task<JToken> Serialize(bool includeAttendees)
		{
			JToken obj = JToken.FromObject(new
			{
				id = ID,
				next_time = NextTime.ToString("dd/MM/yyyy HH:mm"),
				recurrence = SetReccurence
			});

			if (includeAttendees)
			{
				obj["attendees"] = new JArray();

				foreach (Kid attendee in await GetAttendees())
				{
					(obj["attendees"] as JArray).Add(attendee.Serialize());
				}
			}

			return obj;
		}


		public async Task<List<Kid>> GetAttendees()
		{
			Query attendanceQuery = Query.Select("kid_id")
				.From("event_attendance")
				.Where("scheduled_event_id", ID);

			ResultSet result = await Program.MySql().Execute(attendanceQuery);

			List<Kid> kids = new List<Kid>();

			foreach (ResultRow row in result)
			{
				SelectQuery<Kid> kidQuery = new SelectQuery<Kid>();
				kidQuery.Where("kid_id", row.GetInteger("kid_id"));

				Kid kid = await Program.MySql().Execute(kidQuery).ContinueWith(res => res.Result.FirstOrDefault());

				if (kid != null)
					kids.Add(kid);
			}

			return kids;
		}



		[DBColumn(name: "recurrence")]
		public string SetReccurence
		{
			set { Recurrence = ParseRecurrence(value); }
			get { return Recurrence.ToString().ToLower(); }
		}

		static EventRecurrence ParseRecurrence(string reccurence)
		{
			switch (reccurence)
			{
				case "once":
					return EventRecurrence.Once;

				case "daily":
					return EventRecurrence.Daily;

				case "weekly":
					return EventRecurrence.Weekly;

				case "biweekly":
					return EventRecurrence.BiWeekly;

				case "monthly":
					return EventRecurrence.Monthly;

				default:
					goto case "once";
			}
		}
	}

	public enum EventRecurrence
	{
		Once,
		Daily,
		Weekly,
		BiWeekly,
		Monthly
	}
}
