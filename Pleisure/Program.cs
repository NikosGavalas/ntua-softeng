using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnixSignalWaiter;

namespace Pleisure
{
	class Program
	{
		const int PORT = 80;

		static WebServer server;

		static void Main(string[] args)
		{
			server = new WebServer(PORT);
			server.Start();



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
			}
		}

		public static bool IsLinux()
		{
			int p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128);
		}

		static void Shutdown()
		{
			server.Stop();
		}
	}
}
