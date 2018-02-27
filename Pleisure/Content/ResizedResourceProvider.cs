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
	public class ResizedResourceProvider : StaticResourceProvider
	{
		int width;
		int height;

		public ResizedResourceProvider(string resourceRoot, string virtualRoot,
		                              int width, int height) : base(resourceRoot, virtualRoot, ContentType.Image)
		{
			this.width = width;
			this.height = height;
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

			byte[] data = await ReadFile(request, requestedFile + ".png");

			if (data == null)
			{
				// request.SetStatusCode(HttpStatusCode.NotFound);
				// return;
				data = File.ReadAllBytes("app/eventimg/default.png");
			}

			MemoryStream imageStream = new MemoryStream(data);

			Stream resized = MyStamp.ResizeImage(imageStream, width, height);

			byte[] watermarkedData = new byte[resized.Length];
			resized.Read(watermarkedData, 0, (int)resized.Length);

			// Set the appropriate Content-Type
			request.SetContentTypeByExtension(ContentType.Image, "png");

			// If we managed to read the file, first set the headers
			request.SetStatusCode(HttpStatusCode.OK);

			// Then write the file to the response stream
			await request.Write(watermarkedData);
		}
	}
}
