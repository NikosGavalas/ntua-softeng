using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HaathDB;

namespace Pleisure
{
	public class EventScheduler
	{
		volatile bool running;

		public void Start()
		{
			new Thread(() => _Start()).Start();
		}

		async Task _Start()
		{
			running = true;

			

			while (running)
			{
				List<Event> events = await Program.MySql().Select<Event>();
				

				foreach (Event evt in events)
				{
					List<ScheduledEvent> scheduled = await evt.GetScheduled();

					foreach (ScheduledEvent sched in scheduled)
					{
						if (sched.Recurrence != EventRecurrence.Once
							&& sched.NextTime < DateTime.Now)
						{
							if (!scheduled.Any(s => s.Recurrence == sched.Recurrence && s.NextTime > DateTime.Now))
							{
								// Reschedule
								DateTime next = GetNext(sched);
								await Reschedule(sched, next);
							}
						}
					}
				}
				


				await Task.Delay(60 * 1000);
			}
		}

		async Task Reschedule(ScheduledEvent sched, DateTime next)
		{
			InsertQuery query = new InsertQuery("scheduled_events");

			query.Value("event_id", sched.Event.ID)
				 .Value("next_time", next)
				 .Value("recurrence", sched.SetReccurence);

			await Program.MySql().ExecuteNonQuery(query);
		}

		DateTime GetNext(ScheduledEvent evt)
		{
			DateTime next = DateTime.Now;
			switch (evt.Recurrence)
			{
				case EventRecurrence.Daily:
					next.AddDays(1);
					break;

				case EventRecurrence.BiWeekly:
					next.AddDays(14);
					break;

				case EventRecurrence.Monthly:
					next.AddMonths(1);
					break;

				case EventRecurrence.Weekly:
					next.AddDays(7);
					break;
			}
			return next;
		}

		public void Stop()
		{
			running = false;
		}
	}
}
