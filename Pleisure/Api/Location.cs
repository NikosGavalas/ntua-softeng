using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public class Location
	{
		public readonly double Latitude;
		public readonly double Longitude;

		public Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public JToken Serialize()
		{
			return JToken.FromObject(new
			{
				lat = Latitude,
				lng = Longitude
			});
		}

		public override string ToString()
		{
			return Latitude + ", " + Longitude;
		}
	}
}
