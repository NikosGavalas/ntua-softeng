using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			request.SetContentType(ContentType.Json);



			await request.Close();
		}

		public async Task Kids(HttpRequest request)
		{
			request.SetContentType(ContentType.Json);



			await request.Close();
		}
	}
}
