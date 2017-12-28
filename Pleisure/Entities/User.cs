using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;

namespace Pleisure
{
	[DBTable("users")]
	public class User
	{
		[DBColumn("user_id")]
		public uint ID;

		[DBColumn("email")]
		public string Email;

		[DBColumn("full_name")]
		public string FullName;

		[DBColumn("role")]
		public UserRole Role;

		[DBColumn("credits")]
		public int Credits;

		[DBColumn("password")]
		public string Password;

		[DBColumn("salt")]
		public string Salt;

		[DBColumn("address")]
		public string Address;

		public string Avatar 
		{
			get
			{
				return Options.Gravatar(Email);
			}
		}

		public Task<List<Kid>> GetKids()
		{
			SelectQuery<Kid> query = new SelectQuery<Kid>();
			query.Where("parent_id", ID);

			return Program.MySql().Execute(query);
		}

		public Task<List<Event>> GetEvents()
		{
			SelectQuery<Event> query = new SelectQuery<Event>();
			query.Where("organizer_id", ID);

			return Program.MySql().Execute(query);
		}
	}

	public enum UserRole
	{
		Banned		= -1,
		Parent		= 1,
		Organizer	= 2,
		Admin		= 3
	}
}
