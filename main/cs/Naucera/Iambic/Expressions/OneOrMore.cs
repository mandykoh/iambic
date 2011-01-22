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
	/// Expression which accepts a string if its subexpression matches the
	/// string one or more times in succession.
	/// </summary>
	
	public class OneOrMore : ParseExpression
	{
		ParseExpression mExpression;


		/// <summary>
		/// Creates a OneOrMore expression with the specified subexpression.
		/// </summary>
		
		public OneOrMore(ParseExpression expression)
		{
			mExpression = expression;
		}


		/// <summary>
		/// The subexpression.
		/// </summary>
		
		public ParseExpression Expression
		{
			get { return mExpression; }
		}


		internal override bool CheckWellFormed(string baseRuleName, HashSet<string> ruleNames)
		{
			return mExpression.CheckWellFormed(baseRuleName, ruleNames);
		}


		public override void ToString(StringBuilder text)
		{
			mExpression.ToString(text);
			text.Append('+');
		}


		/// <summary>
		/// Compiles the child expression, replacing it with the result.
		/// </summary>
		
		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			mExpression = mExpression.Compile(parser);
			return this;
		}


		internal override bool Parse(ParseContext context, ParseRule rule, out Token result)
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
				state.accepted = mExpression.Parse(context, rule, out state.token);
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


		sealed class OneOrMoreMemento : Memento
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


			OneOrMoreMemento(OneOrMoreMemento m)
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
