using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public class HtmlProvider
	{

		[HtmlVariable("html.navbar")]
		public string Navbar
		{
			get
			{
				return GetHtml("navbar").Result;
			}
		}

		public HtmlProvider()
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
