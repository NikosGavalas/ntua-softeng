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
		/// <summary>
		/// When an exception is raised
		/// </summary>
		Error	= 1 << 0,

		/// <summary>
		/// When something goes wrong without raising an exception
		/// </summary>
		Warning = 1 << 1,

		/// <summary>
		/// For generic info messages regarding the runtime of the server
		/// </summary>
		Info	= 1 << 2,

		/// <summary>
		/// For every request and operation
		/// </summary>
		Debug	= 1 << 3,
	}
}
