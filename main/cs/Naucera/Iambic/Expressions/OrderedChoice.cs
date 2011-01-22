#region license

// Copyright 2011 Amanda Koh. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//    1. Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//
//    2. Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY AMANDA KOH ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL AMANDA KOH OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, either expressed or implied, of Amanda Koh.

#endregion

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

	public class OrderedChoice : CompositeExpression
	{
		/// <summary>
		/// Creates an OrderedChoice from the specified sub-expressions.
		/// </summary>

		public OrderedChoice(params ParseExpression[] expressions) : base(expressions) {}
		
		
		internal override bool CheckWellFormed(string baseRuleName, HashSet<string> ruleNames)
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


		internal override bool Parse(ParseContext context, ParseRule rule, out Token result)
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


		sealed class OrderedChoiceMemento : Memento
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
