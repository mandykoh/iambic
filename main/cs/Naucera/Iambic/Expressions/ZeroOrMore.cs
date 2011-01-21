using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which always accepts a string, matching its subexpression
	/// with the string as many times as possible in sequence.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class ZeroOrMore : ParseExpression
	{
		private ParseExpression expression;


		/// <summary>
		/// Creates a ZeroOrMore expression with the specified subexpression.
		/// </summary>
		
		public ZeroOrMore(ParseExpression expression)
		{
			this.expression = expression;
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			expression.CheckWellFormed(baseRuleName, new HashSet<string>(ruleNames));
			return true;
		}


		internal override ParseExpression Compile(Parser parser)
		{
			expression = expression.Compile(parser);
			return this;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			context.Accept(out result);

			// Parse the expression as many times as possible
			while (true) {
				Token t;
				if (!expression.Parse(context, rule, out t))
					break;

				// Don't keep looping if the token is blank
				if (t.Blank)
					break;

				// If no text was parsed, don't keep looping
				if (t.EndOffset == t.Offset)
					break;

				// Add the parsed token to our result
				result.Add(t);
			}

			if (result.HasChildren)
				result.EndOffset = context.Offset;

			return true;
		}


		public override void ToString(StringBuilder text)
		{
			expression.ToString(text);
			text.Append('*');
		}
   }
}
