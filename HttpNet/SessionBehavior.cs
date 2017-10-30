using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	public abstract class SessionBehavior
	{
		/// <summary>
		/// The unique session id associated with this session
		/// </summary>
		protected string sessionId;

		/// <summary>
		/// The endpoint of the client
		/// </summary>
		protected IPEndPoint remoteEndPoint;

		internal Task OnCreate(string sessionId, IPEndPoint remoteEndPoint)
		{
			this.sessionId = sessionId;
			this.remoteEndPoint = remoteEndPoint;
			return OnCreate();
		}
		/// <summary>
		/// Will fire when this session is created and all the protected fields have been set
		/// </summary>
		/// <returns></returns>
		protected abstract Task OnCreate();

		internal Task OnRequest(string servicePath, HttpRequest request)
		{
			return OnRequest(request);
		}
		protected abstract Task OnRequest(HttpRequest request);
	}
}
