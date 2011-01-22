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
	/// Expression which accepts a string by invoking a custom matcher.
	/// </summary>

	public class CustomMatcherTerminal : ParseExpression
	{
		readonly string mMatcherName;
		CustomMatcher mCustomMatcher;


		/// <summary>
		/// Creates a CustomMatcherTerminal with the specified name. The name
		/// is used to identify a custom matcher referenced in a parser grammar.
		/// </summary>
		
		public CustomMatcherTerminal(string matcherName)
		{
			mMatcherName = matcherName;
		}


		internal override bool CheckWellFormed(string baseRuleName, HashSet<string> ruleNames)
		{
			return false;
		}


		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			mCustomMatcher = parser.CustomMatcher(mMatcherName);
			if (mCustomMatcher == null)
				throw new UndefinedConstructException(mMatcherName);

			return this;
		}


		internal override bool Parse(ParseContext context, ParseRule rule, out Token result)
		{
			if (context.Recovering)
				return context.EndRecovery().Accept(mCustomMatcher, out result);

			// Leniently find the next matching string if we are compensating
			// for an earlier error.
			if (context.Compensating) {
				int offset;
				var length = mCustomMatcher.MatchLeniently(context.BaseText, context.Offset, out offset);

				if (length >= 0) {
					context.Offset = offset;
					return context.EndCompensation().Accept(length, mCustomMatcher, out result);
				}

				return context.Accept(mCustomMatcher, out result);
			}

			// Otherwise, look for a precise match at the current position
			else {
				var length = mCustomMatcher.Match(context.BaseText, context.Offset);

				if (length >= 0)
					return context.Accept(length, mCustomMatcher, out result);
			}

			return context.RejectAndMark(rule, this, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append("{").Append(mMatcherName).Append("}");
		}
	}
}
