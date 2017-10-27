using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	public class Request
	{
		public string Path
		{
			get { return request?.RawUrl.Split('?')[0]; }
		}

		public Dictionary<string, string> GET
		{
			get { return Utils.GetUrlParams(request.RawUrl); } 
		}

		public Dictionary<string, string> POST
		{
			get { return Utils.GetUrlParams("x?" + RequestBody().Result); }
		}

		string _requestBody = null;

		HttpListenerRequest request;
		HttpListenerResponse response;

		StreamWriter responseStream;

		public Request(HttpListenerRequest request, HttpListenerResponse response)
		{
			this.request = request;
			this.response = response;

			responseStream = new StreamWriter(response.OutputStream, Encoding.Default);
		}

		public async Task<string> RequestBody()
		{
			if (_requestBody == null)
			{
				using (StreamReader reader = new StreamReader(request.InputStream, Encoding.Default))
				{
					_requestBody = await reader.ReadToEndAsync();
				}
			}
			return _requestBody;
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
