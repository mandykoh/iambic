#region license

// Copyright 2012 Amanda Koh. All rights reserved.
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
using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class PatternTerminalTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new PatternTerminal("a*b*c*");

			Assert.AreEqual("/a*b*c*/", expr.ToString());
		}


		[Test]
		public void ShouldEscapeRegexStringExpressions()
		{
			Assert.AreEqual(@"/abc\/def\\ghi/", PatternTerminal.Escape(@"abc/def\\ghi"));
		}


		[Test]
		public void ShouldMatchInSingleLineMode()
		{
			const string text = "a\nb\nc\r\n";
			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new PatternTerminal(".*")));

			var t = p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t[0].MatchedText(text));
		}


		[Test]
		public void ShouldMatchTextAtExactOffsetWhenNotRecovering()
		{
			const string text = "abcd";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new PatternTerminal("bcd")));

			try {
				p.Parse(text);

				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldMatchTextLenientlyFromOffsetWhenRecovering()
		{
			const string text = "abcdefg";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new PatternTerminal("bc"),
						new PatternTerminal("de"),
						new PatternTerminal("fg"))))
				{ MaxErrors = 3 };

			try {
				p.Parse(text);

				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
			}
		}


		[Test]
		public void ShouldMatchTextUsingRegex()
		{
			const string text = "abbccc";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new PatternTerminal("a*b*c*d*")));

			var t = p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t[0].MatchedText(text));
		}


		[Test]
		public void ShouldNotAllowInvalidRegex()
		{
			try {
				new PatternTerminal("**");

				Assert.Fail("Expression matched but should not have");
			}
			catch (ArgumentException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldUnescapeRegexStringExpressions()
		{
			Assert.AreEqual(@"abc/def\\ghi", PatternTerminal.Unescape(@"/abc\/def\\ghi/"));
		}
	}
}
