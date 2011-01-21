using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// <para>
	/// Composite expression which represents a prioritised choice of multiple
	/// subexpressions, accepting a string if one of its subexpressions accepts
	/// the string.</para>
	///
	/// <para>
	/// Subexpressions are matched against the input string in the order they
	/// are added.</para>
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>

	public class OrderedChoice : CompositeExpression
	{
		/// <summary>
		/// Creates an OrderedChoice from the specified sub-expressions.
		/// </summary>

		public OrderedChoice(params ParseExpression[] expressions)
			: base(expressions)
		{
		}
		
		
		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			var optional = true;
			var baseRuleNames = new HashSet<string>(ruleNames);

			for (var i = 0; i < ExpressionCount; ++i) {
				var tmp = new HashSet<string>(ruleNames);
				var expr = Expression(i);
				if (!expr.CheckWellFormed(baseRuleName, tmp))
					optional = false;

				foreach (var name in tmp)
					baseRuleNames.Add(name);
			}

			foreach (var name in baseRuleNames)
				ruleNames.Add(name);

			return optional;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			OrderedChoiceMemento state;
			if (context.Recovering)
				state = (OrderedChoiceMemento)context.Recover();
			else
				state = new OrderedChoiceMemento(context);

			context.BeginExpression(state);

			// Parse each subexpression until the first match
			for (; state.index < ExpressionCount; ++state.index) {
				var expr = Expression(state.index);
				Token t;

				// Parsing succeeded on a subexpression - return the result
				if (expr.Parse(context, rule, out t)) {
					context.EndExpression();
					result = t;
					return true;
				}

				context.Offset = state.offset;
			}

			context.EndExpression();

			return context.Reject(rule, this, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append('(');

			for (var i = 0; i < ExpressionCount; ++i) {
				var expr = Expression(i);
				if (i > 0)
					text.Append(" || ");
				expr.ToString(text);
			}

			text.Append(')');
		}


		private sealed class OrderedChoiceMemento : Memento
		{
			internal readonly int offset;
			internal int index;


			internal OrderedChoiceMemento(ParseContext context)
			{
				offset = context.Offset;
			}


			private OrderedChoiceMemento(OrderedChoiceMemento m)
			{
				this.offset = m.offset;
				this.index = m.index;
			}


			internal override Memento Copy()
			{
				return new OrderedChoiceMemento(this);
			}
		}
	}
}
