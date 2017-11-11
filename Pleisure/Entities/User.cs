using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;

namespace Pleisure
{
	public class User
	{
		public int ID { get; private set; }
		public string Email { get; private set; }
		public string FullName { get; private set; }
		public UserRole Role { get; private set; }
		public int Credits { get; private set; }

		public async Task<User> WithId(int userId)
		{
			HaathMySql conn = Program.Mysql();

			Query query = new Query("SELECT email, full_name, role, credits FROM users WHERE user_id=@uid");
			query.AddParameter("@uid", userId);

			DBRow res = await conn.Execute(query).ContinueWith(
				table => table.Result.First()
				);

			return new User()
			{
				ID = userId,
				Email = res.GetString("email"),
				FullName = res.GetString("full_name"),
				Role = (UserRole)res.GetInteger("role"),
				Credits = res.GetInteger("credits")
			};
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
