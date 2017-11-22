using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;

namespace Pleisure
{
	[DBTable("kids")]
	public class Kid
	{
		[DBColumn("kid_id")]
		public int ID;

		[DBColumn("name")]
		public string Name;

		[DBColumn("birthday")]
		public DateTime Birthday;

		[DBColumn("gender")]
		public Gender Gender;

		[DBReference("parent_id", "user_id")]
		public User Parent;
	}

	public enum Gender
	{
		Male = 0,
		Female = 1
	}
}
