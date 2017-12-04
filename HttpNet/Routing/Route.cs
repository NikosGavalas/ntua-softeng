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

		public Route(string relativePath, Func<HttpRequest, Task> handler)
		{
			RelativePath = relativePath;
			Handler = handler;
		}

		public bool MatchesPath(string path)
		{
			string pattern = Utils.WildcardRegex(RelativePath);
			Match match = Regex.Match(path, pattern);
			return match.Success;
		}
	}
}
