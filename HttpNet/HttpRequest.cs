using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

using HttpNet.Extensions;

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

		public CookieCollection Cookies
		{
			get { return request.Cookies; }
		}
		
		byte[] _requestBody = null;

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

		public async Task<byte[]> RequestBody()
		{
			if (_requestBody == null)
			{
				MemoryStream ms = new MemoryStream();
				byte[] buffer = new byte[0xFFFF];

				int i = 0;
				do
				{
					i = await Request.InputStream.ReadAsync(buffer, 0, 0xFFFF);
					ms.Write(buffer, 0, i);
				} while (i > 0);
				ms.Flush();
				ms.Position = 0;

				_requestBody = new byte[ms.Length];
				await ms.ReadAsync(_requestBody, 0, _requestBody.Length);
			}
			return _requestBody;
		}

		public async Task<string> RequestBodyString()
		{
			return Request.ContentEncoding.GetString(await RequestBody());
		}

		public async Task<Stream> RequestStream()
		{
			return new MemoryStream(await RequestBody());
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
			try
			{
				string reqBody = await RequestBodyString();
				Dictionary<string, string> postParams = Utils.GetUrlParams("x?" + reqBody);

				if (postParams.ContainsKey(key))
				{
					return postParams[key];
				}
			}
			catch (Exception) {}

			if (await GetContentData(key) != null)
			{
				StreamReader r = new StreamReader(await GetContentData(key), Request.ContentEncoding);
				return await r.ReadToEndAsync();
			}

			return defaultValue;
		}

		/// <summary>
		/// Checks if all the given keys are present in the POST Parameters.
		/// </summary>
		/// <returns><c>false</c>, if at least one key is missing <c>false</c> otherwise.</returns>
		/// <param name="keys">Keys.</param>
		public async Task<bool> HasPOST(params string[] keys)
		{
			foreach (string key in keys)
			{
				if ((await POST(key)) == null)
					return false;
			}
			return true;
		}

		public Task<MemoryStream> GetContentData(string key)
		{
			return GetContentData<MemoryStream>(key);
		}

		public async Task<T> GetContentData<T>(string key) where T : Stream, new()
		{
			Console.WriteLine(key);

			if (Request.ContentType.Split(';').Length < 2
			    || Request.ContentType.Split(';')[1].Split('=').Length < 2)
				return null;

			string boundary = "--" + Request.ContentType.Split(';')[1].Split('=')[1];
			BinaryReader input = new BinaryReader(await RequestStream(), Request.ContentEncoding);

			string line;
			while ((line = input.ReadLine(Request.ContentEncoding)) != null)
			{
				Regex regex = new Regex(string.Format(@"Content-Disposition: form-data; name=""{0}""", key));
				Match m = regex.Match(line);

				if (m.Success)
				{
					line = input.ReadLine(Request.ContentEncoding);
					if (line.StartsWith("Content-Type:", StringComparison.Ordinal))
					{
						line = input.ReadLine(Request.ContentEncoding); // Skip an extra line
					}


					MemoryStream newStream = await RequestStream() as MemoryStream;

					long endPos = input.BaseStream.Position;
					newStream.Position = input.BaseStream.Position;

					while (line != null && !line.StartsWith(boundary))
					{
						endPos = input.BaseStream.Position;
						line = input.ReadLine(Request.ContentEncoding);
					}

					byte[] buffer = new byte[endPos - newStream.Position];
					newStream.Read(buffer, 0, buffer.Length);


					T outStream = new T();

					outStream.Write(buffer, 0, buffer.Length - 1);
					outStream.Position = 0;
					return outStream;
				}
			}
			return null;
		}

	}
}
