using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpNet
{
	/// <summary>
	/// Common values for the Content-Type HTTP header.
	/// </summary>
	public enum ContentType
	{
		Javascript,
		Html,
		Json,
		Zip,
		Css,
		Plain,
		Image
	}

	/// <summary>
	/// Flags to choose which messages should get logged.
	/// </summary>
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
