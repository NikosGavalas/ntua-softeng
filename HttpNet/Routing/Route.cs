using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HttpNet
{
	internal class Route : IRoutingNode
	{
		public readonly string RelativePath;
		public readonly Func<HttpRequest, Task> Handler;
		public readonly Type SessionBehavior;

		public Route(string relativePath, Func<HttpRequest, Task> handler, Type SessionBehavior)
		{
			RelativePath = relativePath;
			Handler = handler;
			this.SessionBehavior = SessionBehavior;
		}

		public bool MatchesPath(string path)
		{
			string pattern = Utils.WildcardRegex(RelativePath);
			Match match = Regex.Match(path, pattern);
			return match.Success;
		}
	}
}
