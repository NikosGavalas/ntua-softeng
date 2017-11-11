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
		}

		public async Task Kids(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);


			await request.Close();
		}
	}
}
