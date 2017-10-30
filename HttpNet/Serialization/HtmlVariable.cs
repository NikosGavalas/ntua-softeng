using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpNet
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class HtmlVariable : Attribute
	{
		public readonly string Name;

		public HtmlVariable(string name)
		{
			Name = name;
		}
	}
}
