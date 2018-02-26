using System;

using HaathDB;
using Newtonsoft.Json.Linq;

namespace Pleisure
{
	[DBTable("categories")]
	public class Category
	{
		[DBColumn("category_id")]
		public int ID;

		[DBColumn("name")]
		public string Name;

		public JToken Serialize()
		{
			return JToken.FromObject(new
			{
				id = ID,
				name = Name
			});
		}
	}
}
