using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using HttpNet;

using UnixSignalWaiter;

using HaathDB;

namespace Pleisure
{
	class Program
	{
		const string HOST = "*";
		const int PORT = 80;

		static WebServer server;

		static void Main(string[] args)
		{
			server = WebServer.Create<UserSession>(HOST, PORT, sessionLifetime: 300);
			server.LogLevel = LogLevels.All;
			server.OnLog += (s, arg) => Console.WriteLine(arg.Line);
			


			/*
			 * Register content providers
			 */
			StaticResourceProvider css = new StaticResourceProvider(GetPath("app/css"), "/css", ContentType.Css);
			server.Add("/css/*.css", css.OnRequest);

			StaticResourceProvider js = new StaticResourceProvider(GetPath("app/js"), "/js", ContentType.Javascript);
			server.Add("/js/*.js", js.OnRequest);

			StaticResourceProvider png = new StaticResourceProvider(GetPath("app/img"), "/img", ContentType.Image);
			server.Add("/img/*", png.OnRequest);
			

			/*
			 * Register API
			 */
			Api api = new Api(server);


			HtmlProvider pages = new HtmlProvider();
			server.Add("/", pages.Index);
			server.Add("/events", pages.Events);
			server.Add("/event/*", pages.Event);
			server.Add("/profile", pages.Profile);


			server.Start();
			Console.WriteLine("Press CTRL-C to shut down.");

			/*
             * Main thread now awaits SIGTERM
             */
			if (IsLinux())
			{
				SignalWaiter.Instance.WaitExitSignal();
				Shutdown();
			}
			else
			{
				Console.CancelKeyPress += (s, e) => Shutdown();
				while (true)
				{
					Thread.Sleep(1000);
				}
			}
		}

		public static bool IsLinux()
		{
			int p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128);
		}

		static void Shutdown()
		{
			Console.WriteLine("Shutting down...");
			server.Stop();
			Environment.Exit(0);
		}

		public static string GetPath()
		{
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		}

		public static string GetPath(string relative)
		{
			return Path.Combine(GetPath(), relative);
		}

		public static MySqlConn MySql()
		{
			return new MySqlConn("gmantaos.com", "progtech", "@ntua123", "pleisure");
		}
	}
}
