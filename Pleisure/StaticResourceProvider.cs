using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;

using HttpNet;

namespace Pleisure
{
	public class StaticResourceProvider
	{
		string resourceRoot;
		string virtualRoot;
		ContentType contentType;

		public StaticResourceProvider(string resourceRoot, string virtualRoot, ContentType contentType)
		{
			this.resourceRoot = resourceRoot;
			this.virtualRoot = virtualRoot;
			this.contentType = contentType;
		}
		
		public async Task OnRequest(HttpRequest request)
		{
			HttpStatusCode status = await ServeFile(request);
			await request.SetStatusCode(status).Close();
			await request.Close();
		}

		async Task<HttpStatusCode> ServeFile(HttpRequest request)
		{
			if (!request.Path.StartsWith(virtualRoot))
			{
				return HttpStatusCode.Forbidden;
			}
			
			string requestedFile = request.Path.Remove(0, virtualRoot.Length).TrimStart('/');
			FileStream fileStream = null;
			try
			{
				string filePath = Path.Combine(resourceRoot, requestedFile);
				fileStream = File.OpenRead(filePath);
			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
					return HttpStatusCode.NotFound;
				else if (ex is UnauthorizedAccessException)
					return HttpStatusCode.Forbidden;
				else
					Console.WriteLine(ex.ToString());
			}


			byte[] data = new byte[fileStream.Length];
			await fileStream.ReadAsync(data, 0, (int)fileStream.Length);
			await request.Write(data);
			

			// Set the appropriate Content-Type
			switch (contentType)
			{
				case ContentType.Image:
					request.SetContentTypeByExtension(contentType, Path.GetExtension(requestedFile));
					break;

				default:
					request.SetContentType(contentType);
					break;
			}

			return HttpStatusCode.OK;
		}
	}
}
