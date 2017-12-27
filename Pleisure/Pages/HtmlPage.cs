using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public class HtmlPage
	{
		string html;
		User user;

		[HtmlVariable("is_parent")]
		public bool IsParent
		{
			get { return user?.Role == UserRole.Parent; }
		}

		[HtmlVariable("is_organizer")]
		public bool IsOrganizer
		{
			get { return user?.Role == UserRole.Organizer; }
		}

		[HtmlVariable("is_admin")]
		public bool IsAdmin
		{
			get { return user?.Role == UserRole.Admin; }
		}

		[HtmlVariable("is_loggedin")]
		public bool IsLoggedIn
		{
			get { return user != null; }
		}

		[HtmlVariable("user.email")]
		public string UserEmail
		{
			get { return user?.Email; }
		}

		[HtmlVariable("user.name")]
		public string UserName
		{
			get { return user.FullName; }
		}

		[HtmlVariable("user.credits")]
		public int UserCredits
		{
			get { return user != null ? user.Credits : 0; }
		}

		[HtmlVariable("user.address")]
		public string UserAddress
		{
			get { return user != null ? user.Address : ""; }
		}

		[HtmlVariable("html.navbar")]
		public Task<string> Navbar { get { return GetHtml("navbar"); } }

		[HtmlVariable("html.header")]
		public Task<string> Header { get { return GetHtml("header"); } }

		[HtmlVariable("html.footer")]
		public Task<string> Footer { get { return GetHtml("footer"); } }

		[HtmlVariable("modal.login")]
		public Task<string> ModalLogin { get { return GetHtml("modal/login"); } }

		[HtmlVariable("modal.register")]
		public Task<string> ModalRegister { get { return GetHtml("modal/register"); } }

		[HtmlVariable("modal.edit_profile")]
		public Task<string> ModalEditProfile { get { return GetHtml("modal/edit_profile"); } }

		[HtmlVariable("modal.add_kid")]
		public Task<string> ModalAddKid { get { return GetHtml("modal/add_kid"); } }

		[HtmlVariable("modal.add_event")]
		public Task<string> ModalAddEvent { get { return GetHtml("modal/add_event"); } }

		public HtmlPage(string html, User user)
		{
			this.html = html;
			this.user = user;
		}

		public async Task<string> Render()
		{
			HtmlRenderer renderer = new HtmlRenderer(html);
			return await renderer.Render(this);
		}

		async Task<string> GetHtml(string file)
		{
			string fileName = Program.GetPath(string.Format("app/html/{0}.html", file));
			try
			{
				using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
				{
					string fileHtml = await reader.ReadToEndAsync();
					HtmlRenderer renderer = new HtmlRenderer(fileHtml);
					return await renderer.Render(this);
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
