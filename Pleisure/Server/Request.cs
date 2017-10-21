using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Pleisure
{
	public class Request
	{
		HttpListenerRequest request;
		HttpListenerResponse response;
		Dictionary<string, string> getParams;
		Dictionary<string, string> postParams;

		StreamWriter responseStream;

		public Request(HttpListenerRequest request, HttpListenerResponse response,
			Dictionary<string, string> getParams, Dictionary<string, string> postParams)
		{
			this.request = request;
			this.response = response;
			this.getParams = getParams;
			this.postParams = postParams;

			responseStream = new StreamWriter(response.OutputStream, Encoding.Default);
		}

		public Request SetStatusCode(HttpStatusCode code)
		{
			response.StatusCode = (int)code;
			return this;
		}

		public Request SetContentType(ContentType type)
		{
			response.ContentType = Utils.GetContentType(type);
			return this;
		}

		public async Task Close()
		{
			await responseStream.FlushAsync();
			responseStream.Close();
			responseStream.Dispose();
		}
	}
}
