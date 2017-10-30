using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HttpNet
{
	public class HtmlRenderer
	{
		char[] htmlDocument;

		public HtmlRenderer(string htmlDocument)
		{
			this.htmlDocument = htmlDocument.ToCharArray();
		}

		public string Render(object obj)
		{
			StringBuilder rendered = new StringBuilder();

			State state = State.Writing;
			int i = 0;

			while (i < htmlDocument.Length - 1)
			{
				switch (state)
				{
					case State.Writing:
						rendered.Append(htmlDocument[i]);
						break;

					case State.TrueIf:
						break;

					case State.FalseIf:
						break;

					case State.EndIf:
						break;
				}
			}

			return rendered.ToString();
		}

		static object GetHtmlVariable(object obj, string name)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties();

			foreach (PropertyInfo prop in properties)
			{
				HtmlVariable val = prop.GetCustomAttribute<HtmlVariable>();

				if (val != null && val.Name == name)
				{
					return prop.GetValue(obj);
				}
			}

			FieldInfo[] fields = obj.GetType().GetFields();

			foreach (FieldInfo field in fields)
			{
				HtmlVariable val = field.GetCustomAttribute<HtmlVariable>();

				if (val != null && val.Name == name)
				{
					return field.GetValue(obj);
				}
			}

			return null;
		}

		enum State
		{
			Writing,
			TrueIf,
			FalseIf,
			EndIf
		}
	}
}
