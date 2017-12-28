using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

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
		public Dictionary<string, string> GETParams
		{
			get { return Utils.GetUrlParams(request.RawUrl); } 
		}

		/// <summary>
		/// The url-encoded POST parameters in the body of the request.
		/// <para>WARNING: await reading the RequestBody() first!!!</para>
		/// </summary>
		public Dictionary<string, string> POSTParams
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

		public HttpListenerRequest Request { get { return request; } }
		public HttpListenerResponse Response { get { return response; } }

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
		/// Set the response to redirect to the given url and close the response stream.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public Task Redirect(string url)
		{
			response.Redirect(url);
			return Close();
		}

		/// <summary>
		/// Returns the GET parameter with the given key, or the defaultValue if it doesnt exist.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue">The value to return if this parameter doesn't exist</param>
		/// <returns></returns>
		public string GET(string key, string defaultValue = null)
		{
			return GETParams.ContainsKey(key) ? GETParams[key] : defaultValue;
		}

		/// <summary>
		/// Checks if all the given keys are present in the GET Parameters.
		/// </summary>
		/// <returns><c>false</c>, if at least one key is missing <c>false</c> otherwise.</returns>
		/// <param name="keys">Keys.</param>
		public bool HasGET(params string[] keys)
		{
			foreach (string key in keys)
			{
				if (!GETParams.ContainsKey(key))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the POST parameter with the given key, or the defaultValue if it doesnt exist.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue">The value to return if this parameter doesn't exist</param>
		/// <returns></returns>
		public async Task<string> POST(string key, string defaultValue = null)
		{
			await RequestBody();
			if (POSTParams.ContainsKey(key))
			{
				return POSTParams[key];
			}

			if (GetContentData(key) != null)
			{
				StreamReader r = new StreamReader(await GetContentData(key));
				return await r.ReadToEndAsync();
			}

			return defaultValue;
		}

		/// <summary>
		/// Checks if all the given keys are present in the POST Parameters.
		/// </summary>
		/// <returns><c>false</c>, if at least one key is missing <c>false</c> otherwise.</returns>
		/// <param name="keys">Keys.</param>
		public bool HasPOST(params string[] keys)
		{
			foreach (string key in keys)
			{
				if (!POSTParams.ContainsKey(key))
					return false;
			}
			return true;
		}

		public async Task<Stream> GetContentData(string key)
		{
			string boundary = "--" + Request.ContentType.Split(';')[1].Split('=')[1];

			string[] lines = (await RequestBody()).Split('\n');

			for (int i = 0; i < lines.Length; i++)
			{
				Regex regex = new Regex(string.Format(@"Content-Disposition: form-data; name=""{0}""", key));
				Match m = regex.Match(lines[i]);

				if (m.Success)
				{
					i++;
					if (lines[i].StartsWith("Content-Type:"))
						i += 2;
					else
						i++;

					StringBuilder output = new StringBuilder();

					while (i < lines.Length && !lines[i].StartsWith("----------------------------"))
					{
						output.AppendLine(lines[i++]);
					}

					string str = output.ToString();
					str = str.Substring(0, str.Length - 1);
					MemoryStream outStream = new MemoryStream(Request.ContentEncoding.GetBytes(str));
					outStream.Position = 0;
					return outStream;
				}
			}
			return null;
		}

	}
}
