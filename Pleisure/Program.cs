using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

using HttpNet;

using UnixSignalWaiter;

namespace Pleisure
{
	class Program
	{
		const string HOST = "*";
		const int PORT = 80;

		static WebServer server;

		static void Main(string[] args)
		{
			server = new WebServer(HOST, PORT, sessionLifetime: 300);
			server.LogLevel = server.LogLevel | LogLevels.Debug | LogLevels.Info;
			server.OnLog += (s, arg) => Console.WriteLine(arg.Line);

			StaticResourceProvider css = new StaticResourceProvider(GetPath("app/css"), "/css", ContentType.Css);
			server.AddResource("/css/*.css", css.OnRequest);



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
					Thread.Sleep(5000);
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
	}
}
