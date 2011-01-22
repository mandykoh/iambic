using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which always accepts the input string, optionally matching
	/// its subexpression against the string if possible.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class Optional : ParseExpression
	{
		private ParseExpression expression;


		/// <summary>
		/// Creates an Optional expression with the specified subexpression.
		/// </summary>
		
		public Optional(ParseExpression expression)
		{
			this.expression = expression;
		}


		/// <summary>
		/// The subexpression.
		/// </summary>
		
		public ParseExpression Expression {
			get { return expression; }
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			expression.CheckWellFormed(baseRuleName, new HashSet<string>(ruleNames));
			return true;
		}


		/// <summary>
		/// Compiles the child expression, replacing it with the result.
		/// </summary>
		
		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			expression = expression.Compile(parser);
			return this;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			var recovering = context.Recovering;

			Token token;

			if (expression.Parse(context, rule, out token)) {
				result = token;
				return true;
			}

			if (recovering)
				return context.Reject(rule, this, out result);

			return context.Accept(out result);
		}


		public override void ToString(StringBuilder text)
		{
			expression.ToString(text);
			text.Append('?');
		}
	}
}
