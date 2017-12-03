using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using XXHash;
using HaathDB;

namespace Pleisure
{
	public static class Auth
	{
		private static string Hash(string plain)
		{
			ulong hash = XXHash64.Hash(plain);
			byte[] buffer = BitConverter.GetBytes(hash);
			return BitConverter.ToString(buffer).Replace("-", "").ToLower();
		}

		private static string GenerateSalt()
		{
			const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(
				Enumerable.Repeat(pool, 32)
					.Select(s => s[StaticRandom.Rand(min:0, max: s.Length)])
					.ToArray()
				);
		}

		private static string GetPasswordHash(string password, string salt)
		{
			StringBuilder soup = new StringBuilder();

			for (int i = 0; i < password.Length || i < salt.Length; i++)
			{
				if (i < password.Length)
					soup.Append(password[i]);
				if (i < salt.Length)
					soup.Append(salt[i]);
			}

			return Hash(soup.ToString());
		}

		public static async Task<bool> EmailTaken(string email)
		{
			SelectQuery query = new SelectQuery("user_id");
			query.From("users")
				.Where("email", email);

			ResultSet result = await Program.MySql().Execute(query);

			return result.RowCount > 0;
		}

		public static async Task<long> RegisterUser(string email, string password, string fullName, UserRole role, int credits = 0)
		{
			if (await EmailTaken(email))
			{
				return -1;
			}

			string salt = GenerateSalt();
			string passwordHash = GetPasswordHash(password, salt);

			InsertQuery query = new InsertQuery("users");
			query.Value("email", email)
				.Value("password", passwordHash)
				.Value("salt", salt)
				.Value("full_name", fullName)
				.Value("role", (int)role)
				.Value("credits", credits);

			NonQueryResult result = await Program.MySql().ExecuteNonQuery(query);

			if (result.RowsAffected != 1)
			{
				Console.WriteLine("Tried to register user and got rows affected: " + result.RowsAffected);
				Console.WriteLine(query.QueryString());
			}

			return result.LastInsertedId;
		}



		private static class StaticRandom
		{
			static int seed = Environment.TickCount;

			static readonly ThreadLocal<Random> random =
				new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

			public static int Rand(int min = int.MinValue, int max = int.MaxValue)
			{
				return random.Value.Next(min, max);
			}
		}
	}
}
