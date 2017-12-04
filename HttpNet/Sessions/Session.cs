using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	internal class Session
	{
		public readonly string SessionID;
		public readonly IPEndPoint RemoteEndPoint;
		public SessionBehavior Behavior = null;

		Stopwatch timer;

		internal bool Valid = true;

		public Session(string sessionId, IPEndPoint remoteEndPoint)
		{
			SessionID = sessionId;
			RemoteEndPoint = remoteEndPoint;

			timer = Stopwatch.StartNew();
		}

		public double ElapsedSeconds()
		{
			return timer.Elapsed.TotalSeconds;
		}

		internal Cookie GetCookie()
		{
			return new Cookie("SESSID", SessionID);
		}

		internal void Update()
		{
			timer.Restart();
		}

		internal void Destroy()
		{
			Valid = false;
		}
	}
}
