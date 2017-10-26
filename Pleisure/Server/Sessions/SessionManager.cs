using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;

namespace Pleisure.Server
{
	public class SessionManager
	{
		int sessionLifeTimeSeconds;

		Dictionary<string, Session> _sessions;
		Dictionary<string, Session> sessions
		{
			get
			{
				_sessions = _sessions.Where(s => s.Value.ElapsedSeconds() <= sessionLifeTimeSeconds).ToDictionary(k => k.Key, v => v.Value);
				return _sessions;
			}
		}

		public SessionManager(int sessionLifeTimeSeconds)
		{
			this.sessionLifeTimeSeconds = sessionLifeTimeSeconds;
			_sessions = new Dictionary<string, Session>();
		}

		public Session GetSessionWithId(string sessionId)
		{
			Session session = null;
			sessions.TryGetValue(sessionId, out session);
			return session;
		}

		public Session CreateSession(IPEndPoint remoteEndPoint)
		{
			string sessionId = GenerateSessionId(remoteEndPoint);
			Session session = new Session(sessionId, remoteEndPoint);
			sessions.Add(sessionId, session);
			return session;
		}

		string GenerateSessionId(IPEndPoint remoteEndPoint)
		{
			string seed = string.Format("{0}.{1}", remoteEndPoint.Address.ToString(), DateTime.UtcNow.ToString());
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));

			StringBuilder sessionId = new StringBuilder();
			foreach (byte b in hash)
			{
				sessionId.Append(b.ToString("x2"));
			}
			return sessionId.ToString();
		}
	}
}
