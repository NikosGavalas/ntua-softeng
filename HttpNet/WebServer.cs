using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	/// <summary>
	/// 
	/// </summary>
	public class WebServer
	{
		HttpListener server;
		SessionManager sessionManager;

		volatile bool running;
		
		public event EventHandler<LogEventArgs> OnLog;
		public event Action<Exception> OnException;
		public LogLevels LogLevel;

		int sessionLifetime;

		public WebServer(string host, int port, int sessionLifetime = 300)
		{
			this.sessionLifetime = sessionLifetime;

			server = new HttpListener();
			server.Prefixes.Add(string.Format("http://{0}:{1}/", host, port));

			LogLevel = LogLevels.Warning | LogLevels.Error;
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
					Session session = GetOrSetSession(connection.Request, connection.Response);
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

			Request req = new Request(request, response);
			await req.Close();
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
	}
}
