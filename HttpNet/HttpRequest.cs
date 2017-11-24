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
		Session _session;
		public SessionBehavior Session
		{
			get { return _session.Behavior; }
			internal set { _session.Behavior = value; }
		}

		/// <summary>
		/// The requested resource of the request
		/// </summary>
		public string AbsolutePath
		{
			get { return request?.RawUrl.Split('?')[0]; }
		}

		public string Path { get; internal set; }

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

		WebServer webServer;
		HttpListenerRequest request;
		HttpListenerResponse response;

		internal HttpRequest(WebServer webServer, HttpListenerRequest request, HttpListenerResponse response, Session session)
		{
			this.webServer = webServer;
			this.request = request;
			this.response = response;
			_session = session;

			Path = AbsolutePath;
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

		public HttpRequest SetContentTypeByExtension(ContentType type, string extension)
		{
			response.ContentType = Utils.GetContentType(type) + extension;
			return this;
		}

		public HttpRequest SetHeader(string name, string value)
		{
			response.AddHeader(name, value);
			return this;
		}

		/// <summary>
		/// Write data asynchronously on the response stream.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="encoding">The encoding of the text</param>
		/// <returns></returns>
		public Task Write(string data, Encoding encoding)
		{
			return Write(encoding.GetBytes(data));
		}

		/// <summary>
		/// Write data asynchronously on the response stream.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task Write(string data)
		{
			return Write(data, Encoding.Default);
		}

		/// <summary>
		/// Write data asynchronously on the response stream.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task Write(byte[] data)
		{
			return response.OutputStream.WriteAsync(data, 0, data.Length);
		}

		public Task Write(char character)
		{
			return Write(character, Encoding.Default);
		}

		public Task Write(char character, Encoding encoding)
		{
			return Write(character.ToString(), encoding);
		}

		/// <summary>
		/// Flush and close the response stream.
		/// </summary>
		/// <returns></returns>
		public async Task Close()
		{
			await response.OutputStream.FlushAsync();
			response.OutputStream.Close();
			response.OutputStream.Dispose();

			webServer.Log(LogLevels.Debug, "Closing request {0} [{1}] [{2}]", AbsolutePath, response.StatusCode, response.ContentType);
		}

		/// <summary>
		/// Shortcut to setting the Content-Type, the StatusCode to 200 and closing the stream.
		/// </summary>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public async Task Success(ContentType contentType)
		{
			SetContentType(contentType);
			SetStatusCode(HttpStatusCode.OK);
			await Close();
		}
	}
}
