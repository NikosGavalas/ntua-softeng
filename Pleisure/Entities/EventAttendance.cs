using System;

using HaathDB;

namespace Pleisure
{
	[DBTable("event_attendance")]
	public class EventAttendance
	{
		[DBColumn("scheduled_event_id")]
		public int ScheduledID;

		[DBColumn("kid_id")]
		public int KidID;

		public EventAttendance() {}

		public EventAttendance(ScheduledEvent evt, Kid kid)
		{
			ScheduledID = evt.ID;
			KidID = kid.ID;
		}
	}
}
