using System;
using System.IO;
using System.Text;
using System.Linq;

namespace HttpNet.Extensions
{
	public static class Extensions
	{
		public static Stream ToStream(this string str)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static string ReadLine(this BinaryReader reader, Encoding encoding, bool includeNewLine = false)
		{
			if (reader.BaseStream.Position == reader.BaseStream.Length)
				return null;

			byte[] newLine = encoding.GetBytes("\n");

			StringBuilder line = new StringBuilder();

			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				byte[] buf = reader.ReadBytes(newLine.Length);

				if (buf.SequenceEqual(newLine))
				{
					if (includeNewLine)
						line.Append("\n");

					break;
				}

				line.Append(encoding.GetString(buf));
			}

			return line.ToString();
		}
	}
}
