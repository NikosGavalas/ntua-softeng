using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;

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
