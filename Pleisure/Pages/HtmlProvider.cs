using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public class HtmlProvider
	{

		public async Task Index(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await GetUser(session);

			string html = await GetHtml("index");
			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);

			await request.Success(ContentType.Html);
		}

		public async Task Events(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await GetUser(session);
			
			string html = await GetHtml("events");
			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);
			
			await request.Success(ContentType.Html);
		}

		public async Task Event(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await GetUser(session);
			int eventId = GetEventId(request.Path);


			string html = await GetHtml("event");
			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();

			// TODO: also render over Event


			await request.Write(rendered);

			await request.Success(ContentType.Html);
		}

		public async Task Profile(HttpRequest request)
		{
			UserSession session = request.Session as UserSession;
			User user = await GetUser(session);

			if (!session.LoggedIn)
			{
				request.SetHeader("Location", "/");
				await request.Close();
				return;
			}


			string html = await GetHtml("profile");
			HtmlPage page = new HtmlPage(html, user);

			string rendered = await page.Render();
			await request.Write(rendered);

			await request.Success(ContentType.Html);
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

		async Task<User> GetUser(UserSession session)
		{
			if (session.LoggedIn)
			{
				return null;
			}
			else
			{
				return null;
			}
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
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
	}
}
