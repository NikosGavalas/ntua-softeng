using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using HttpNet;
using Watermark;
using HaathDB;

namespace Pleisure
{
	public class WatermarkedResourceProvider : StaticResourceProvider
	{
		public WatermarkedResourceProvider(string resourceRoot, string virtualRoot) : base(resourceRoot, virtualRoot, ContentType.Image)
		{
		}

		protected override async Task ServeFile(HttpRequest request)
		{
			string requestedFile = request.Path.Remove(0, virtualRoot.Length).TrimStart('/');
			int eventId;

			if (!int.TryParse(requestedFile, out eventId))
			{
				request.SetStatusCode(HttpStatusCode.BadRequest);
				return;
			}

			SelectQuery<Event> query = new SelectQuery<Event>()
				.Where<SelectQuery<Event>>("event_id", eventId);

			Event evt = (await Program.MySql().Execute(query)).FirstOrDefault();

			byte[] data = await ReadFile(request, requestedFile + ".png");

			if (data == null)
			{
				// request.SetStatusCode(HttpStatusCode.NotFound);
				// return;
				data = File.ReadAllBytes("app/eventimg/default.png");
			}
			
			MemoryStream imageStream = new MemoryStream(data);


			MyStamp stamp = new MyStamp()
			{
				Opacity		= 100,
				Shadow		= true
			};

			Stream watermarked = stamp.ApplyStamp(imageStream, evt.Organizer.FullName);

			byte[] watermarkedData = new byte[watermarked.Length];
			watermarked.Read(watermarkedData, 0, (int)watermarked.Length);

			// Set the appropriate Content-Type
			request.SetContentTypeByExtension(ContentType.Image, "png");

			// If we managed to read the file, first set the headers
			request.SetStatusCode(HttpStatusCode.OK);

			// Then write the file to the response stream
			await request.Write(watermarkedData);
		}
	}
}
