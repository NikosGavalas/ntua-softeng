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
		public string Navbar { get { return GetHtml("navbar").Result; } }

		[HtmlVariable("html.header")]
		public string Header { get { return GetHtml("header").Result; } }

		[HtmlVariable("html.footer")]
		public string Footer { get { return GetHtml("footer").Result; } }

		[HtmlVariable("modal.login")]
		public string ModalLogin { get { return GetHtml("modal/login").Result; } }

		[HtmlVariable("modal.register")]
		public string ModalRegister { get { return GetHtml("modal/register").Result; } }

		[HtmlVariable("modal.edit_profile")]
		public string ModalEditProfile { get { return GetHtml("modal/edit_profile").Result; } }

		[HtmlVariable("modal.add_kid")]
		public string ModalAddKid { get { return GetHtml("modal/add_kid").Result; } }

		[HtmlVariable("modal.add_event")]
		public string ModalAddEvent { get { return GetHtml("modal/add_event").Result; } }

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
