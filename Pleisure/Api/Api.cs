using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;
using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public class Api
	{

		public Api(Router router)
		{
			router.Add("/kids", Kids);
			router.Add("/events", Events);
		}

		public async Task Kids(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);


			await request.Close();
		}

		public async Task Events(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);

			JArray arr = new JArray();

			SelectQuery<Event> query = new SelectQuery<Event>();

			List<Event> events = await Program.MySql().Execute(query);
			foreach (Event evt in events)
			{
				arr.Add(evt.Serialize());
			}

			await request.Write(arr.ToString());

			await request.Close();
		}


	}
}
