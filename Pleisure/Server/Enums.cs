using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pleisure.Server
{
	public enum ContentType
	{
		Javascript,
		Html,
		Json,
		Zip,
		Css,
		Plain
	}

	[Flags]
	public enum LogLevels
	{
		Error	= 1 << 0,
		Warning = 1 << 1,
		Info	= 1 << 2,
		Debug	= 1 << 3,
	}
}
