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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which accepts a string if it matches a regular expression
	/// pattern.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2011 by Amanda Koh.</para>
	/// </remarks>

	public class PatternTerminal : ParseExpression
	{
		private readonly string pattern;
		private readonly Regex regex;


		/// <summary>
		/// Creates a Terminal expression which matches the specified regular
		/// expression. The regular expression is compiled using the standard
		/// System.Text.RegularExpressions implementation.
		/// </summary>
		/// 
		/// <exception cref="ArgumentException">
		/// Thrown if the regular expression pattern is invalid.</exception>

		public PatternTerminal(string pattern)
		{
			this.pattern = pattern;
			this.regex = new Regex(@"\G" + pattern, RegexOptions.Singleline);
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			return false;
		}


		private Regex CompensationRegex {
			get { return new Regex(pattern); }
		}


		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			return this;
		}


		/// <summary>
		/// Escapes the specified string and returns the grammar regex literal
		/// that would match it.
		/// </summary>

		public static string Escape(string text)
		{
			var pattern = new StringBuilder();

			pattern.Append('/');

			for (var i = 0; i < text.Length; ++i) {
				var c = text[i];

				switch (c) {
					case '/': pattern.Append(@"\/"); break;
					default: pattern.Append(c); break;
				}
			}

			pattern.Append('/');

			return pattern.ToString();
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			if (context.Recovering)
				return context.EndRecovery().Accept(out result);

			// Leniently find the next matching string if we are compensating
			// for an earlier error.
			if (context.Compensating) {
				var match = CompensationRegex.Match(context.BaseText, context.Offset);

				if (match.Success) {
					context.Offset = match.Index;
					return context.EndCompensation().Accept(match.Length, out result);
				}

				return context.Accept(out result);
			}

			// Otherwise, look for a precise match at the current position
			else {
				var match = regex.Match(context.BaseText, context.Offset);
				if (match.Success)
					return context.Accept(match.Length, out result);
			}

			return context.RejectAndMark(rule, this, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append(Escape(pattern));
		}


		/// <summary>
		/// Unescapes the specified grammar regex literal and returns the
		/// literal string that would be matched. The result of calling this on
		/// an invalid grammar regex literal is undefined.
		/// </summary>

		public static string Unescape(string pattern)
		{
			var text = new StringBuilder();

			for (var i = 1; i < pattern.Length - 1; ++i) {
				var c = pattern[i];

				if (c == '\\' && i < pattern.Length - 2) {
					var next = pattern[i + 1];
					if (next == '/') {
						text.Append(next);
						++i;
						continue;
					}
				}

				text.Append(c);
			}

			return text.ToString();
		}
	}
}
