using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpNet
{
	public class LogEventArgs : EventArgs
	{
		public readonly LogLevels Level;
		public readonly string Message;
		public readonly DateTime Timestamp;

		public string Line
		{
			get
			{
				return  string.Format("[{0}]({1}) {2}",
					Utils.GetLogLevelTag(Level),
					Timestamp.ToString("yyyy/mm/dd HH:mm:ss"),
					Message);
			}
		}

		internal LogEventArgs(LogLevels level, string message)
		{
			Level = level;
			Message = message;
			Timestamp = DateTime.Now;
		}
	}
}
