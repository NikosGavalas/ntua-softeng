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
			await ServeFile(request);
			await request.Close();
		}

		async Task ServeFile(HttpRequest request)
		{
			if (!request.Path.StartsWith(virtualRoot))
			{
				request.SetStatusCode(HttpStatusCode.Forbidden);
                return;
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
                {
                    request.SetStatusCode(HttpStatusCode.NotFound);
                    return;
                }
				else if (ex is UnauthorizedAccessException)
                {
                    request.SetStatusCode(HttpStatusCode.Forbidden);
                    return;
                }
				else
				{
					Console.WriteLine(ex);
                    request.SetStatusCode(HttpStatusCode.InternalServerError);
                    return;
                }
			}

			byte[] data = new byte[fileStream.Length];
			await fileStream.ReadAsync(data, 0, (int)fileStream.Length);


            // If we managed to read the file, first set the headers
            request.SetStatusCode(HttpStatusCode.OK);

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


            // Then write the file to the response stream
			await request.Write(data);
		    
            // Dispose of the file
			fileStream.Close();
			fileStream.Dispose();
		}
	}
}
