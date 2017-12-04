using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpNet;
using HaathDB;

namespace Pleisure
{
	public class UserSession : SessionBehavior
	{
		public long UserID = -1;

		public bool LoggedIn
		{
			get { return UserID > -1; }
		}

		public async Task<User> GetUser()
		{
			if (LoggedIn)
			{
				SelectQuery<User> query = new SelectQuery<User>()
					.Where<SelectQuery<User>>("user_id", UserID);
				return await Program.MySql().Execute(query).ContinueWith(res => res.Result.FirstOrDefault());
			}
			else
			{
				return null;
			}
		}
	}
}
