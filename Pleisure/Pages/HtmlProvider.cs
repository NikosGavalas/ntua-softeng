using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;
using ChanceNET;

namespace Pleisure
{
	public class HtmlProvider
	{

		public async Task Index(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await session.GetUser();

			string html = await GetHtml("index");
			if (html == null)
			{
				await request.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			request.SetStatusCode(HttpStatusCode.OK);
			request.SetContentType(ContentType.Html);

			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);

			await request.Close();
		}

		public async Task Events(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await session.GetUser();
			
			string html = await GetHtml("events");
			if (html == null)
			{
				await request.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			request.SetStatusCode(HttpStatusCode.OK);
			request.SetContentType(ContentType.Html);

			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);

			await request.Close();
		}

		public async Task Event(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await session.GetUser();
			int eventId = GetEventId(request.Path);

			SelectQuery<Event> query = new SelectQuery<Event>()
				.Where<SelectQuery<Event>>("event_id", eventId);

			Event evt = (await Program.MySql().Execute(query)).FirstOrDefault();

#if DEBUG
			if (evt == null)
			{
				Chance c = new Chance();
				evt = Pleisure.Event.Random(eventId, c.Latitude(), c.Longitude(), c.Natural());
			}

#endif


			string html = await GetHtml("event");
			if (html == null || eventId < 0 || evt == null)
			{
				await request.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			request.SetStatusCode(HttpStatusCode.OK);
			request.SetContentType(ContentType.Html);

			HtmlPage page = new EventPage(html, user, evt);

			string rendered = await page.Render();


			await request.Write(rendered);

			await request.Close();
		}

		public async Task Profile(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await session.GetUser();

			if (!session.LoggedIn)
			{
				await request.Redirect("/");
				return;
			}


			string html = await GetHtml("profile");
			if (html == null)
			{
				await request.SetStatusCode(HttpStatusCode.NotFound).Close();
				return;
			}

			request.SetStatusCode(HttpStatusCode.OK);
			request.SetContentType(ContentType.Html);

			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);

			await request.Close();
		}

		int GetEventId(string requestUrl)
		{
			int id = -1;
			Match match = Regex.Match(requestUrl, @"/event/(\d+)$");

			if (match.Success)
			{
				int.TryParse(match.Groups[1].Value, out id);
			}

			return id;
		}

		async Task<string> GetHtml(string page)
		{
			string fileName = Program.GetPath(string.Format("app/{0}.html", page));
			try
			{
				using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
				{
					return await reader.ReadToEndAsync();
				}
			}
			catch (Exception)
			{
				Console.WriteLine("File not found: " + fileName);
				return null;
			}
		}
	}
}
