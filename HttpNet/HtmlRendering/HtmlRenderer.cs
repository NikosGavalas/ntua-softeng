using System;
using System.Collections.Generic;
using System.IO;
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

		public async Task<string> Render(object obj)
		{
			StringBuilder rendered = new StringBuilder();

			State state = State.Writing;
			int i = 0;

			Func<bool> isOpenTag = () =>
			{
				return i < htmlDocument.Length - 1
					&& htmlDocument[i] == '@'
					&& htmlDocument[i + 1] == '{';
			};

			Func<bool> isIfTag = () =>
			{
				return i < htmlDocument.Length - 3
					&& htmlDocument[i] == '@'
					&& htmlDocument[i + 1] == 'i'
					&& htmlDocument[i + 2] == 'f'
					&& htmlDocument[i + 3] == '{';
			};

			Func<bool> isElseTag = () =>
			{
				return i < htmlDocument.Length - 6
					&& htmlDocument[i] == '@'
					&& htmlDocument[i + 1] == 'e' && htmlDocument[i + 2] == 'l'
					&& htmlDocument[i + 3] == 's' && htmlDocument[i + 4] == 'e'
					&& htmlDocument[i + 5] == '{' && htmlDocument[i + 6] == '}'; ;
			};

			Func<bool> isEndifTag = () =>
			{
				return i < htmlDocument.Length - 6
					&& htmlDocument[i] == '@'
					&& htmlDocument[i + 1] == 'e' && htmlDocument[i + 2] == 'n'
					&& htmlDocument[i + 3] == 'd' && htmlDocument[i + 4] == 'i'
					&& htmlDocument[i + 5] == 'f' && htmlDocument[i + 6] == '{'
					&& htmlDocument[i + 7] == '}';
			};

			Func<string> parseVariableName = () =>
			{
				// Get over the opening tag
				while (htmlDocument[i] == '@' || htmlDocument[i] == '{')
					i++;

				StringBuilder var = new StringBuilder();
				while (htmlDocument[i] != '}')
				{
					var.Append(htmlDocument[i]);
					i++;
				}

				// Get over the closing tag
				i++;

				return var.ToString();
			};

			while (i < htmlDocument.Length)
			{
				switch (state)
				{
					case State.Writing:
						if (isIfTag())
						{
							i += 3;
							string varName = parseVariableName();
							bool negate = varName.StartsWith("!");
							varName = varName.TrimStart('!');

							object varValue = await GetHtmlVariable(obj, varName);

							if (varValue == null)
							{
								throw new MissingFieldException("Missing boolean field: " + varName);
							}

							if ((bool)varValue ^ negate)
							{
								state = State.TrueIf;
							}
							else
							{
								state = State.FalseIf;
								break;
							}
						}
						if (isOpenTag())
						{
							string varName = parseVariableName();
							object varValue = await GetHtmlVariable(obj, varName);

							if (varValue == null)
							{
								// Log warning?
								rendered.Append("@{" + varName + "}");
							}
							else
							{
								rendered.Append(varValue.ToString());
							}
						}

						if (i < htmlDocument.Length)
							rendered.Append(htmlDocument[i++]);

						break;

					case State.TrueIf:
						if (isEndifTag())
						{
							goto case State.EndIf;
						}
						else if (isElseTag())
						{
							state = State.FalseIf;
							i += 6;
							break;
						}
						else
						{
							goto case State.Writing;
						}

					case State.FalseIf:
						if (isEndifTag())
						{
							goto case State.EndIf;
						}
						else if (isElseTag())
						{
							state = State.TrueIf;
							i += 6;
						}
						i++;
						break;

					case State.EndIf:
						if (isEndifTag())
						{
							i += 8;
							state = State.Writing;
						}
						i++;
						break;
				}
			}

			return rendered.ToString();
		}

		static async Task<object> GetHtmlVariable(object obj, string name)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties();

			foreach (PropertyInfo prop in properties)
			{
				HtmlVariable val = prop.GetCustomAttribute<HtmlVariable>();

				if (val != null && val.Name == name)
				{
					return await GetAsync(prop.GetValue(obj));
				}
			}

			FieldInfo[] fields = obj.GetType().GetFields();

			foreach (FieldInfo field in fields)
			{
				HtmlVariable val = field.GetCustomAttribute<HtmlVariable>();

				if (val != null && val.Name == name)
				{
					return await GetAsync(field.GetValue(obj));
				}
			}

			return null;
		}

		static async Task<object> GetAsync(object obj)
		{
			if (obj.GetType().IsGenericType
				&& obj.GetType().GetGenericTypeDefinition() == typeof(Task<>))
			{
				try
				{
					return await (Task<string>)obj;
				}
				catch (Exception) { }

				try
				{
					return await (Task<int>)obj;
				}
				catch (Exception) { }
				
				try
				{
					return await (Task<double>)obj;
				}
				catch (Exception) { }
				
				try
				{
					return await (Task<bool>)obj;
				}
				catch (Exception) { }

				try
				{
					return await (Task<float>)obj;
				}
				catch (Exception) { }

				return obj;
			}
			else
			{
				return obj;
			}
		}

		enum State
		{
			Writing,
			TrueIf,
			FalseIf,
			EndIf
		}

		enum FlowTag
		{
			None,
			If,
			Elseif,
			Else,
			Endif
		}
	}
}
