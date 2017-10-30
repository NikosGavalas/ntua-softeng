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
			request.SetStatusCode(status);
			await request.Close();
		}

		async Task<HttpStatusCode> ServeFile(HttpRequest request)
		{
			if (!request.Path.StartsWith(virtualRoot))
			{
				return HttpStatusCode.Forbidden;
			}
			
			string requestedFile = request.Path.Remove(0, virtualRoot.Length).TrimStart('/');
			StreamReader fileStream = null;
			try
			{
				string filePath = Path.Combine(resourceRoot, requestedFile);
				FileStream stream = File.OpenRead(filePath);
				fileStream = new StreamReader(stream);
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


			string body = await fileStream.ReadToEndAsync();
			await request.Write(body);

			request.SetContentType(contentType);
			return HttpStatusCode.OK;
		}
	}
}
