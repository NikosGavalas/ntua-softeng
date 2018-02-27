using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

using XXHash;
using HaathDB;

namespace Pleisure
{
	public static class Auth
	{
		static object coherenceLock;

		static Auth()
		{
			coherenceLock = new object();
		}

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

		/// <summary>
		/// Checks if an email is already taken by another user.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public static async Task<bool> EmailTaken(string email)
		{
			SelectQuery query = new SelectQuery("user_id");
			query.From("users")
				.Where("email", email);

			ResultSet result = await Program.MySql().Execute(query);

			return result.RowCount > 0;
		}

		/// <summary>
		/// Registers a user and returns his ID.
		/// <para>Returns -1 if the email is already taken.</para>
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="fullName"></param>
		/// <param name="role"></param>
		/// <param name="credits"></param>
		/// <returns></returns>
		public static async Task<long> RegisterUser(string email, string password, string fullName, 
		                                            int role, string address = "", int credits = 0)
		{
			Monitor.Enter(coherenceLock);

			if (await EmailTaken(email))
			{
				Monitor.Exit(coherenceLock);
				return -1;
			}

			string salt = GenerateSalt();
			string passwordHash = GetPasswordHash(password, salt);

			InsertQuery query = new InsertQuery("users");
			query.Value("email", email)
				.Value("password", passwordHash)
				.Value("salt", salt)
				.Value("full_name", fullName)
				.Value("role", role)
		     	.Value("address", address)
				.Value("credits", credits);

			NonQueryResult result = await Program.MySql().ExecuteNonQuery(query);

			if (result.RowsAffected != 1)
			{
				Console.WriteLine("Tried to register user and got rows affected: " + result.RowsAffected);
				Console.WriteLine(query.QueryString());
			}

			Monitor.Exit(coherenceLock);

			return result.LastInsertedId;
		}

		/// <summary>
		/// Attempt to login with an email and a password.
		/// <para>On success the User object is returned, and null is returned on failure.</para>
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static async Task<User> Authenticate(string email, string password)
		{
			SelectQuery<User> query = Query.Select<User>();
			query.Where("email", email);

			List<User> result = await Program.MySql().Execute(query);

			if (result.Count == 0)
			{
				return null;
			}

			User user = result.First();

			string givenHash = GetPasswordHash(password, user.Salt);

			return (user.Password == givenHash && user.Role != UserRole.Banned) ? user : null;
		}

		public static bool ValidateEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
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

		public static byte[] MD5(byte[] text)
		{
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			return md5.ComputeHash(text);
		}

		public static string MD5(string text, Encoding encoding)
		{
			byte[] input = encoding.GetBytes(text);
			byte[] hash = MD5(input);

			StringBuilder hex = new StringBuilder();

			for (int i = 0; i < hash.Length; i++)
			{
				hex.Append(hash[i].ToString("x2"));
			}

			return hex.ToString();
		}

		public static string MD5(string text)
		{
			return MD5(text, Encoding.ASCII);
		}

		public static async Task<bool> BookEvent(User user, Kid kid, ScheduledEvent scheduled)
		{
			Monitor.Enter(coherenceLock);

			bool success = false;

			Event evt = scheduled.Event;
			User organizer = evt.Organizer;

			if (user.Credits > evt.Price)
			{
				/*
				 * Transfer funds
				 */
				UpdateQuery<User> chargeQuery = new UpdateQuery<User>();
				chargeQuery.Where("user_id", user.ID);
				chargeQuery.Set("credits", user.Credits - evt.Price);

				UpdateQuery<User> compensateQuery = new UpdateQuery<User>();
				compensateQuery.Where("user_id", organizer.ID);
				compensateQuery.Set("credits", organizer.Credits + evt.Price);

				await Program.MySql().Execute(chargeQuery);
				await Program.MySql().Execute(compensateQuery);

				/*
				 * Add attendance
				 */
				EventAttendance attendance = new EventAttendance(scheduled, kid);
				await Program.MySql().Insert(attendance);

				success = true;
			}

			Monitor.Exit(coherenceLock);

			return success;
		}

		public static async Task BanUser(User user)
		{
			Monitor.Enter(coherenceLock);
			
			UpdateQuery<User> query = new UpdateQuery<User>();
			query.Where("user_id", user.ID);
			query.Set("role", UserRole.Banned);
			await Program.MySql().Execute(query);

			Monitor.Exit(coherenceLock);
		}

		public static async Task<bool> VerifyPayment(string ccNum, string ccName, string ccExp, string cvv, int amount)
		{
			return true;
		}

		public static async Task AddCredits(User user, int amount)
		{
			Monitor.Enter(coherenceLock);

			UpdateQuery<User> query = new UpdateQuery<User>();
			query.Where("user_id", user.ID);
			query.Set("credits", user.Credits + amount);
			await Program.MySql().Execute(query);

			Monitor.Exit(coherenceLock);
		}
	}
}
