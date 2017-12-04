using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpNet;

namespace Pleisure
{
	public class UserSession : SessionBehavior
	{
		public long UserID = -1;

		public bool LoggedIn
		{
			get { return UserID > -1; }
		}
	}
}
