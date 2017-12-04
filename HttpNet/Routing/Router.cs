using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HttpNet
{
	public class Router : IRoutingNode
	{
		string path;
		WebServer server;

		List<Router> routers;
		List<Route> routes;

		internal Router(WebServer server, string path)
		{
			this.server = server;
			this.path = path;

			routers = new List<Router>();
			routes = new List<Route>();
		}

		internal async Task Handle(HttpRequest request, Session session)
		{
			if (!MatchesPath(request.Path))
			{
				throw new ArgumentException(string.Format("Router {0} does not match path {1}", path, request.Path));
			}

			request.Path = request.Path.Substring(path.Length);

			foreach (Route route in routes)
			{
				if (route.MatchesPath(request.Path))
				{
					if (session.Behavior == null 
						&& (route.SessionBehavior == typeof(SessionBehavior) || route.SessionBehavior.IsSubclassOf(typeof(SessionBehavior))))
					{
						session.Behavior = (SessionBehavior)Activator.CreateInstance(route.SessionBehavior);
						await session.Behavior.OnCreate(session, session.SessionID, session.RemoteEndPoint);
					}

					server.Log(LogLevels.Debug, string.Format("Handling path {0} at route {1}", request.Path, route.RelativePath));

					await route.Handler(request);
					return;
				}
			}

			foreach (Router router in routers)
			{
				if (router.MatchesPath(request.Path))
				{
					server.Log(LogLevels.Debug, string.Format("Forwarding path {0} to router at {1}", request.Path, router.path));

					await router.Handle(request, session);
					return;
				}
			}

			request.SetStatusCode(HttpStatusCode.NotFound);
			await request.Close();
		}

		public Router Add<Behavior>(string path, Func<HttpRequest, Task> handler)
			where Behavior : SessionBehavior, new()
		{
			if (routes.Exists(r => r.MatchesPath(path)))
			{
				Route c = routes.First(r => r.MatchesPath(path));
				throw new ArgumentException(string.Format("Cannot add {0} to router {1} because of conflict with existing route {2}.", path, this.path, c.RelativePath));
			}

			routes.Add(new Route(path, handler, typeof(Behavior)));
			server.Log(LogLevels.Debug, string.Format("Added path {0} to router at {1}", path, this.path + "/"));
			return this;
		}

		public Router Add(string path, Func<HttpRequest, Task> handler)
		{
			return Add<SessionBehavior>(path, handler);
		}

		public Router CreateRouter(string path)
		{
			Router router = new Router(server, path);
			routers.Add(router);
			return router;
		}

		public bool MatchesPath(string path)
		{
			return (this.path == "" || path.StartsWith(this.path))
				&& path.Substring(this.path.Length).StartsWith("/");
		}
	}
}
