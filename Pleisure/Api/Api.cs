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
			router.Add("/users", Users);
		}

		public async Task Users(HttpRequest request)
		{
			request.SetContentType(ContentType.Html);
			
			await request.Write("<img src='/img/logo.png'>");

			await request.Close();
		}

		public async Task Kids(HttpRequest request)
		{
			request.SetContentType(ContentType.Html);

			await request.Write(request.Session.SessionID);

			await request.Close();
		}
	}
}
