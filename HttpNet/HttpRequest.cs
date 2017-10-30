using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	public class HttpRequest
	{
		/// <summary>
		/// The requested resource of the request
		/// </summary>
		public string Path
		{
			get { return request?.RawUrl.Split('?')[0]; }
		}

		/// <summary>
		/// The GET parameters sent with the request
		/// </summary>
		public Dictionary<string, string> GET
		{
			get { return Utils.GetUrlParams(request.RawUrl); } 
		}

		/// <summary>
		/// The url-encoded POST parameters in the body of the request.
		/// <para>WARNING: await reading the RequestBody() first!!!</para>
		/// </summary>
		public Dictionary<string, string> POST
		{
			get { return Utils.GetUrlParams("x?" + RequestBody().Result); }
		}

		public CookieCollection Cookies
		{
			get { return request.Cookies; }
		}

		string _requestBody = null;

		HttpListenerRequest request;
		HttpListenerResponse response;

		StreamWriter responseStream;

		internal HttpRequest(HttpListenerRequest request, HttpListenerResponse response)
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

		public HttpRequest AddCookie(Cookie cookie)
		{
			Cookies.Add(cookie);
			return this;
		}

		public HttpRequest SetStatusCode(HttpStatusCode code)
		{
			response.StatusCode = (int)code;
			return this;
		}

		public HttpRequest SetContentType(ContentType type)
		{
			response.ContentType = Utils.GetContentType(type);
			return this;
		}

		/// <summary>
		/// Write data asynchronously on the response stream
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task Write(string data)
		{
			return responseStream.WriteAsync(data);
		}

		/// <summary>
		/// Flush and close the response stream.
		/// </summary>
		/// <returns></returns>
		public async Task Close()
		{
			await responseStream.FlushAsync();
			responseStream.Close();
			responseStream.Dispose();
		}
	}
}
