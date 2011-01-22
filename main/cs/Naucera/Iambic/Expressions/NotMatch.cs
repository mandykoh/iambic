using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which accepts a string, consuming no input, only if its
	/// subexpression does not accept the string at the current position.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class NotMatch : ParseExpression
	{
		private ParseExpression expression;


		/// <summary>
		/// Creates a NotMatch expression for the specified subexpression.
		/// </summary>
		
		public NotMatch(ParseExpression expression)
		{
			this.expression = expression;
		}


		/// <summary>
		/// The subexpression.
		/// </summary>
		
		public ParseExpression Expression {
			get { return expression; }
		}


		public override void ToString(StringBuilder text)
		{
			text.Append('!');
			expression.ToString(text);
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			return expression.CheckWellFormed(baseRuleName, ruleNames);
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
			if (context.Recovering) {
				context.EndRecovery();
				return context.Accept(out result);
			}

			var offset = context.Offset;
			Token token;
			var accepted = expression.Parse(context, rule, out token);

			context.ClearRecovery();

			if (accepted) {
				context.Offset = offset;
				return context.RejectAndMark(rule, this, out result);
			}

			return context.Accept(out result);
		}
	}
}
