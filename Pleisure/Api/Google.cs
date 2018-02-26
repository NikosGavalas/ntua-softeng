using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Pleisure
{
	public static class Google
	{
		const string GEOCODE_URL = @"https://maps.googleapis.com/maps/api/geocode/json?key=AIzaSyCyWLrMenGVpUiC44SbWedRvsO4cinN4FU&address=";

		public static async Task<Location> Geocode(string address)
		{
			address = HttpUtility.UrlEncode(address);
			WebRequest request = WebRequest.Create(GEOCODE_URL + address);
			request.Method = "GET";

			WebResponse response = await request.GetResponseAsync();

			StreamReader reader;
			try
			{
				reader = new StreamReader(response.GetResponseStream());
			}
			catch (Exception)
			{
				return null;
			}

			JToken respObj = JToken.Parse(await reader.ReadToEndAsync());

			JArray results = respObj["results"] as JArray;

			if (results.Count == 0)
			{
				return null;
			}

			JToken location = results[0]["geometry"]["location"];

			double lat = location.Value<double>("lat");
			double lng = location.Value<double>("lng");

			return new Location(lat, lng);
		}
	}
}
