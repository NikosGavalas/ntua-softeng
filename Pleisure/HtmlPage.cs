using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public abstract class HtmlPage
	{

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

		public HtmlPage()
		{

		}

		async Task<string> GetHtml(string file)
		{
			string fileName = Program.GetPath(string.Format("app/html/{0}.html", file));
			using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
			{
				string html = await reader.ReadToEndAsync();
				HtmlRenderer renderer = new HtmlRenderer(html);
				return await renderer.Render(this);
			}
		}
	}
}
