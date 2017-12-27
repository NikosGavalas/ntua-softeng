using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
using Newtonsoft.Json.Linq;

using ChanceNET;

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

		public int Age
		{
			get
			{
				int age = DateTime.Now.Year - Birthday.Year;
				if (DateTime.Now.DayOfYear < Birthday.DayOfYear)
					age--;
				return age;
			}
		}

		public JToken Serialize()
		{
			Chance c = new Chance(ID);
			return JToken.FromObject(new
			{
				kid_id = ID,
				name = Name,
				age = Age,
				gender = (int)Gender,
				parent = new
				{
					id = Parent.ID,
					name = Parent.FullName
				},
				avatar = c.Avatar(GravatarDefaults.Identicon)
			});
		}
	}

	public enum Gender
	{
		Male = 0,
		Female = 1
	}
}
