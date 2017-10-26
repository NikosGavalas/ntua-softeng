using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;

using Newtonsoft.Json.Linq;

namespace Pleisure.Server
{
	public class WebServer
	{
		HttpListener server;

		volatile bool running;

		public event Action<string> OnLog;
		public event Action<Exception> OnException;
		public LogLevels LogLevel { get; private set; }

		public WebServer(string host, int port)
		{
			server = new HttpListener();
			server.Prefixes.Add(string.Format("http://{0}:{1}/", host, port));
		}

		public void Start()
		{
			running = true;
			server.Start();
			ServerLoop();
		}

		public void Stop()
		{
			running = false;
			server.Stop();
		}

		async void ServerLoop()
		{
			while (running)
			{
				try
				{
					HttpListenerContext connection = await server.GetContextAsync();
					Request(connection);
				}
				catch (Exception ex)
				{
					OnException?.Invoke(ex);
				}
			}
		}

		async void Request(HttpListenerContext connection)
		{
			HttpListenerRequest request = connection.Request;
			HttpListenerResponse response = connection.Response;

			string requestPath = request.RawUrl.Split('?')[0];
			Dictionary<string, string> getParams = HttpGetParams(request.RawUrl);
			Dictionary<string, string> postParams = await HttpPostParams(request);

			Request req = new Request(request, response, getParams, postParams);
		}

		public static Dictionary<string, string> HttpGetParams(string rawUrl)
		{
			Dictionary<string, string> GET = new Dictionary<string, string>();

			string[] urlParts = rawUrl.Split('?');

			if (urlParts.Length < 2 || urlParts[1] == "")
				return GET;

			NameValueCollection vals = HttpUtility.ParseQueryString(urlParts[1]);

			foreach (string key in vals.AllKeys)
			{
				GET.Add(key, vals.Get(key));
			}

			return GET;
		}

		/// <summary>
		/// Parses the body of the request for POST parameters. Assumes that the parameters will either be URL-encoded or in JSON format.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static async Task<Dictionary<string, string>> HttpPostParams(HttpListenerRequest request)
		{
			string postData;
			using (StreamReader requestStream = new StreamReader(request.InputStream, Encoding.Default))
			{
				postData = await requestStream.ReadToEndAsync();
			}

			JObject PostObj;
			try
			{
				PostObj = JObject.Parse(postData);
			}
			catch
			{
				return HttpGetParams("x?" + postData);
			}

			Dictionary<string, string> POST = new Dictionary<string, string>();

			foreach (KeyValuePair<string, JToken> Pair in PostObj)
			{
				POST.Add(Pair.Key, Pair.Value.ToString());
			}

			return POST;
		}
	}
}
