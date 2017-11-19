using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using HttpNet;

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

			Random rand = new Random();
			for (int i = 0; i < 10; i++)
			{
				JToken evt = JToken.FromObject(new
				{
					id = i,
					title = "We like kids!",
					price = rand.Next(10, 100),
					coordinates = new
					{
						lat = Math.Round(59 + rand.NextDouble(), 6),
						lng = Math.Round(37 + rand.NextDouble(), 6),
					},
					description = "We really do love all the kids",
					duration = 120
				});
				arr.Add(evt);
			}

			await request.Write(arr.ToString());

			await request.Close();
		}


	}
}
