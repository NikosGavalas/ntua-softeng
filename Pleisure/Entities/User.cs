using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HaathDB;
using ChanceNET;
using Newtonsoft.Json.Linq;

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

		public async Task<JToken> Serialize()
		{
			JToken token = JToken.FromObject(new {
				id			= ID,
				email		= Email,
				fullname	= FullName,
				role		= new
				{
					code	= (int)Role,
					title	= Role.ToString().ToLower()
				},
				credits		= Credits,
				address		= Address
			});

			switch (Role)
			{
				case UserRole.Parent:
					token["kids"] = new JArray();
					foreach (Kid kid in await GetKids())
					{
						(token["kids"] as JArray).Add(await kid.Serialize());
					}
					break;

				case UserRole.Organizer:
					token["events"] = new JArray();
					foreach (Event evt in await GetEvents())
					{
						(token["events"] as JArray).Add(await evt.Serialize());
					}
					break;
			}

			return token;
		}

		public static User Random(Chance chance, UserRole? role = null)
		{
			return new User()
			{
				ID			= (uint)chance.Natural(),
				Email		= chance.Email(),
				FullName	= chance.FullName(prefix: true, middle: true, middleInitial: true),
				Role		= role ?? chance.PickEnum<UserRole>(),

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
