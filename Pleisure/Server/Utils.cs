using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pleisure.Server
{
	public static class Utils
	{
		public static string GetContentType(ContentType type)
		{
			switch (type)
			{
				case ContentType.Css:
					return "text/css";

				case ContentType.Html:
					return "text/html";

				case ContentType.Javascript:
					return "application/javascript";

				case ContentType.Json:
					return "application/json";

				case ContentType.Plain:
					return "text/plain";

				case ContentType.Zip:
					return "application/zip";

				default:
					goto case ContentType.Plain;
			}
		}

		public static string GetLogLevelTag(LogLevels level)
		{
			switch (level)
			{
				case LogLevels.Debug:
					return "DEBUG";

				case LogLevels.Error:
					return "ERROR";

				case LogLevels.Info:
					return "INFO";

				case LogLevels.Warning:
					return "WARNING";

				default:
					return "";
			}
		}
	}
}
