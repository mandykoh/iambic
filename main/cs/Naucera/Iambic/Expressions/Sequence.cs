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
	/// Composite expression which evaluates a sequence of nested expressions,
	/// accepting a string if it matches each expression in turn, and rejecting
	/// it otherwise.
	/// </summary>
	
	public class Sequence : CompositeExpression
	{
		/// <summary>
		/// Creates a Sequence from the specified sub-expressions.
		/// </summary>

		public Sequence(params ParseExpression[] expressions)
			: base(expressions)
		{
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			for (var i = 0; i < ExpressionCount; ++i) {
				var tmp = new HashSet<string>(ruleNames);
				if (!Expression(i).CheckWellFormed(baseRuleName, tmp)) {
					foreach (var name in tmp)
						ruleNames.Add(name);
					return false;
				}
			}

			return true;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			SequenceMemento state;
			if (context.Recovering)
				state = (SequenceMemento)context.Recover();
			else
				state = new SequenceMemento(context);

			context.BeginExpression(state);

			// Parse each subexpression to find nested tokens
			for (; state.index < ExpressionCount; ++state.index) {
				var expr = Expression(state.index);
				Token t;

				// Parsing failed on a subexpression - return an overall failure
				if (!expr.Parse(context, rule, out t)) {
					context.Offset = state.offset;
					context.EndExpression();
					result = t;
					return false;
				}

				// Skip blank tokens
				if (t.Blank)
					continue;

				// Add the parsed token to our result
				state.result.Add(t);
			}

			if (state.result.HasChildren)
				state.result.EndOffset = context.Offset;

			context.EndExpression();

			result = state.result;
			return true;
		}


		public override void ToString(StringBuilder text)
		{
			text.Append('(');

			for (var i = 0; i < ExpressionCount; ++i) {
				var expr = Expression(i);
				if (i > 0)
					text.Append(' ');
				expr.ToString(text);
			}

			text.Append(')');
		}


		private sealed class SequenceMemento : Memento
		{
			internal readonly Token result;
			internal int index;
			internal readonly int offset;


			public SequenceMemento(ParseContext context)
			{
				context.Accept(out result);
				offset = context.Offset;
			}


			private SequenceMemento(SequenceMemento m)
			{
				this.result = m.result;
				this.index = m.index;
				this.offset = m.offset;
			}


			internal override Memento Copy()
			{
				return new SequenceMemento(this);
			}
		}
	}
}
