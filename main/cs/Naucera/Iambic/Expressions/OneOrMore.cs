using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which accepts a string if its subexpression matches the
	/// string one or more times in succession.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class OneOrMore : ParseExpression
	{
		private ParseExpression expression;


		/// <summary>
		/// Creates a OneOrMore expression with the specified subexpression.
		/// </summary>
		
		public OneOrMore(ParseExpression expression)
		{
			this.expression = expression;
		}


		/// <sumary>
		/// The subexpression.
		/// </summary>
		
		public ParseExpression Expression {
			get { return expression; }
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			return expression.CheckWellFormed(baseRuleName, ruleNames);
		}


		public override void ToString(StringBuilder text)
		{
			expression.ToString(text);
			text.Append('+');
		}


		/// <summary>
		/// Compiles the child expression, replacing it with the result.
		/// </summary>
		
		internal override ParseExpression Compile(Parser parser)
		{
			expression = expression.Compile(parser);
			return this;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			var recovering = context.Recovering;

			OneOrMoreMemento state;
			if (recovering)
				state = (OneOrMoreMemento)context.Recover();
			else
				state = new OneOrMoreMemento(context);

			context.BeginExpression(state);

			// Parse the expression as many times as possible
			while (true) {
				state.accepted = expression.Parse(context, rule, out state.token);
				if (!state.accepted) {
					if (recovering && state.token.Offset != state.offset && state.result.HasChildren) {
						context.EndExpression();
						return context.Reject(rule, this, out result);
					}
					break;
				}

				// Don't keep looping if the token is blank
				if (state.token.Blank)
					break;

				// Add the parsed token to our result
				state.result.Add(state.token);
				state.offset = context.Offset;

				// If no text was parsed, don't keep looping
				if (state.token.EndOffset == state.token.Offset)
					break;
			}

			context.EndExpression();

			// Check that the expression was parsed at least once
			if (!state.result.HasChildren) {
				if (recovering && state.accepted) {
					result = state.result;
					return true;
				}
				return context.Reject(rule, this, out result);
			}

			state.result.EndOffset = context.Offset;
			result = state.result;
			return true;
		}


		private sealed class OneOrMoreMemento : Memento
		{
			internal readonly Token result;
			internal bool accepted;
			internal Token token;
			internal int offset;


			internal OneOrMoreMemento(ParseContext context)
			{
				context.Accept(out result);
				offset = context.Offset;
			}


			private OneOrMoreMemento(OneOrMoreMemento m)
			{
				this.result = m.result;
				this.accepted = m.accepted;
				this.token = m.token;
				this.offset = m.offset;
			}


			internal override Memento Copy()
			{
				return new OneOrMoreMemento(this);
			}
		}
	}
}
