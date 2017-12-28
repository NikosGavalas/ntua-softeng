using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pleisure
{
	public static class Options
	{
		public static string Host 
		{
			get 
			{
				return Environment.GetEnvironmentVariable("HTTP_HOST") ?? "*";
			}
		}

		public static int Port
		{
			get
			{
				return int.Parse(Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080");
			}
		}

		public static string MysqlHost
		{
			get
			{
				return Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "gmantaos.com";
			}
		}

		public static int MysqlPort
		{
			get
			{
				return int.Parse(Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306");
			}
		}

		public static string MysqlUser
		{
			get
			{
				return Environment.GetEnvironmentVariable("MYSQL_USER") ?? "progtech";
			}
		}

		public static string MysqlPass
		{
			get
			{
				return Environment.GetEnvironmentVariable("MYSQL_PASS") ?? "@ntua123";
			}
		}

		public static string MysqlDb
		{
			get
			{
				return Environment.GetEnvironmentVariable("MYSQL_DB") ?? "pleisure";
			}
		}

		public static string Gravatar(string email)
		{
			return string.Format("https://www.gravatar.com/avatar/{0}?d=retro", Auth.MD5(email));
		}
	}
}
