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
		protected string resourceRoot;
		protected string virtualRoot;
		ContentType contentType;

		public StaticResourceProvider(string resourceRoot, string virtualRoot, ContentType contentType)
		{
			this.resourceRoot = resourceRoot;
			this.virtualRoot = virtualRoot;
			this.contentType = contentType;
		}
		
		public async Task OnRequest(HttpRequest request)
		{
			if (!request.Path.StartsWith(virtualRoot))
			{
				request.SetStatusCode(HttpStatusCode.Forbidden);
			}
			else
			{
				await ServeFile(request);

			}
			await request.Close();
		}

		protected async virtual Task ServeFile(HttpRequest request)
		{
			string requestedFile = request.Path.Remove(0, virtualRoot.Length).TrimStart('/');

			byte[] data = await ReadFile(request, requestedFile);

			if (data == null)
			{
				return;
			}
			
            // Then write the file to the response stream
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

			// If we managed to read the file, first set the headers
			request.SetStatusCode(HttpStatusCode.OK);

		}

		protected async Task<byte[]> ReadFile(HttpRequest request, string requestedFile)
		{
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
					return null;
				}
				else if (ex is UnauthorizedAccessException)
				{
					request.SetStatusCode(HttpStatusCode.Forbidden);
					return null;
				}
				else
				{
					Console.WriteLine(ex);
					request.SetStatusCode(HttpStatusCode.InternalServerError);
					return null;
				}
			}

			byte[] data = new byte[fileStream.Length];
			await fileStream.ReadAsync(data, 0, (int)fileStream.Length);

			// Dispose of the file
			fileStream.Close();
			fileStream.Dispose();

			return data;
		}
	}
}
