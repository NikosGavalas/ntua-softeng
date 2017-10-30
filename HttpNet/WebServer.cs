using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace HttpNet
{
	public class WebServer
	{
		HttpListener server;
		SessionManager sessionManager;

		volatile bool running;
		
		public event EventHandler<LogEventArgs> OnLog;
		public event Action<Exception> OnException;
		public LogLevels LogLevel;

		int sessionLifetime;

		Dictionary<string, Type> services;
		Dictionary<string, Func<HttpRequest, Task>> resources;

		public WebServer(string host, int port, int sessionLifetime = 300)
		{
			this.sessionLifetime = sessionLifetime;

			server = new HttpListener();
			server.Prefixes.Add(string.Format("http://{0}:{1}/", host, port));

			LogLevel = LogLevels.Warning | LogLevels.Error;

			services = new Dictionary<string, Type>();
			resources = new Dictionary<string, Func<HttpRequest, Task>>();
		}

		public void AddService<Behavior>(string path) 
			where Behavior : SessionBehavior, new()
		{
			services.Add(path, typeof(SessionBehavior));
		}

		public void AddResource(string path, Func<HttpRequest, Task> handler)
		{
			resources.Add(path, handler);
		}

		public void Start()
		{
			Log(LogLevels.Info, "Web Server started on: " + server.Prefixes.First());

			running = true;

			sessionManager = new SessionManager(sessionLifetime);
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
					HandleRequest(connection);
				}
				catch (Exception ex)
				{
					OnException?.Invoke(ex);
				}
			}
		}

		void HandleRequest(HttpListenerContext connection)
		{
			HttpRequest request = new HttpRequest(connection.Request, connection.Response);

			Log(LogLevels.Debug, "Request: " + request.Path);

			bool requestHandled = false;
			
			// TODO: Multiple handlers handling the same request?
			// perhaps the best-matching path should handle it
			foreach (KeyValuePair<string, Type> service in ServicesForUrl(request.Path))
			{
				// Any session behavior services
				Session session = GetOrSetSession(connection.Request, connection.Response);

				HandleServiceRequest(session, request, service.Key, service.Value);
				requestHandled = true;

				Log(LogLevels.Debug, request.Path + " handled by service at " + service.Key);
			}

			foreach (KeyValuePair<string, Func<HttpRequest, Task>> resource in ResourcesForUrl(request.Path))
			{
				// Any resource handlers
				resource.Value?.Invoke(request);
				requestHandled = true;

				Log(LogLevels.Debug, request.Path + " handled by resource at " + resource.Key);
			}

			if (!requestHandled)
			{
				request.SetStatusCode(HttpStatusCode.ServiceUnavailable);
				request.Close();
			}
		}

		async Task HandleServiceRequest(Session session, HttpRequest request, 
			string servicePath, Type behaviorType)
		{
			if (session.Behavior == null && behaviorType == typeof(SessionBehavior))
			{
				session.Behavior = (SessionBehavior)Activator.CreateInstance(behaviorType);
				await session.Behavior.OnCreate(session.SessionID, session.RemoteEndPoint);
			}

			if (session.Behavior != null)
			{
				await session.Behavior.OnRequest(servicePath, request);
			}
		}

		Session GetOrSetSession(HttpListenerRequest request, HttpListenerResponse response)
		{
			Session session = null;
			Cookie sessCookie = request.Cookies["SESSID"];

			if (sessCookie != null)
			{
				session = sessionManager.GetSessionWithId(sessCookie.Value);
			}

			if (session == null)
			{
				session = sessionManager.CreateSession(request.RemoteEndPoint);
				Log(LogLevels.Debug, "New session: " + session.SessionID);
			}

			sessCookie = session.GetCookie();
			sessCookie.Expires = DateTime.Now + TimeSpan.FromSeconds(sessionLifetime);
			response.SetCookie(sessCookie);
			return session;
		}

		void Log(LogLevels level, string message)
		{
			if (LogLevel.HasFlag(level))
			{
				OnLog?.Invoke(this, new LogEventArgs(level, message));
			}
		}

		IEnumerable<KeyValuePair<string, Type>> ServicesForUrl(string rawUrl)
		{
			string path = rawUrl.Split('?')[0];
			foreach (KeyValuePair<string, Type> service in services)
			{
				if (Match(service.Key, path))
					yield return service;
			}
		}

		IEnumerable<KeyValuePair<string, Func<HttpRequest, Task>>> ResourcesForUrl(string path)
		{
			foreach (KeyValuePair<string, Func<HttpRequest, Task>> resource in resources)
			{
				if (Match(resource.Key, path))
					yield return resource;
			}
		}

		bool Match(string pattern, string path)
		{
			pattern = Utils.WildcardRegex(pattern);
			Match match = Regex.Match(path, pattern);
			return match.Success;
		}


	}
}
