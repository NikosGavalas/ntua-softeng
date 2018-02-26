using System;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public class EventPage : HtmlPage
	{
		Event evt;

		[HtmlVariable("event.id")]
		public int EventId
		{
			get { return evt.ID; }
		}

		[HtmlVariable("event.title")]
		public string EventTitle
		{
			get { return evt?.Title; }
		}

		[HtmlVariable("event.description")]
		public string EventDescription
		{
			get { return evt?.Description; }
		}

		[HtmlVariable("event.address")]
		public string EventAddress
		{
			get { return evt?.Address; }
		}

		[HtmlVariable("event.lat")]
		public double EventLatitude
		{
			get { return evt.Latitude; }
		}

		[HtmlVariable("event.lng")]
		public double EventLongitude
		{
			get { return evt.Longitude; }
		}

		[HtmlVariable("event.price")]
		public int EventPrice
		{
			get { return evt.Price; }
		}

		[HtmlVariable("event.serialized")]
		public string Serialized
		{
			get { return evt.SerializeWithScheduled().Result.ToString(); }
		}

		[HtmlVariable("event.organizer_name")]
		public string EventOrganizer
		{
			get { return evt?.Organizer.FullName; }
		}

		[HtmlVariable("event.organizer_avatar")]
		public string EventOrganizerAvatar
		{
			get { return evt?.Organizer.Avatar; }
		}

		[HtmlVariable("event.organizer_id")]
		public uint EventOrganizerId
		{
			get { return evt.Organizer.ID; }
		}

		[HtmlVariable("event.age_min")]
		public int EventAgeMin
		{
			get { return evt.AgeMin; }
		}

		[HtmlVariable("event.age_max")]
		public int EventAgeMax
		{
			get { return evt.AgeMax; }
		}

		[HtmlVariable("event.image")]
		public string EventImage
		{
			get { return "/eventimg/" + evt.ID; }
		}

		[HtmlVariable("event.duration")]
		public int EventDuration
		{
			get { return evt.Duration; }
		}

		[HtmlVariable("event.genders")]
		public string EventGenders
		{
			get { return evt.Genders.ToString(); }
		}

		public EventPage(string html, User user, Event evt) : base(html, user)
		{
			this.evt = evt;
		}
	}
}
