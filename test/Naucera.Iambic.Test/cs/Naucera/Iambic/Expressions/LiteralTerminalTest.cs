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

using Xunit;

namespace Naucera.Iambic.Expressions
{
	public class LiteralTerminalTest
	{
		[Fact]
		public void ShouldConvertToGrammarString()
		{
			var expr = new LiteralTerminal("D'Angelo");

			Assert.Equal("'D\\'Angelo'", expr.ToString());
		}


		[Fact]
		public void ShouldEscapeLiteralStringExpressions()
		{
			Assert.Equal("'They\\'re \\\\'", LiteralTerminal.Escape("They're \\"));
		}


		[Fact]
		public void ShouldMatchPrefixOfText()
		{
			const string text = "applepie";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal("apple")));
			var t = p.Parse(text);

			Assert.Equal(0, t.Offset);
			Assert.Equal(5, t.EndOffset);
			Assert.Equal(1, t.ChildCount);
			Assert.Equal("apple", t[0].MatchedText(text));
		}


		[Fact]
		public void ShouldMatchTextAtExactOffsetWhenNotRecovering()
		{
			const string text = "apple";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal("apple")));
			var t = p.Parse(text);

			Assert.Equal(0, t.Offset);
			Assert.Equal(5, t.EndOffset);
			Assert.Equal(1, t.ChildCount);
			Assert.Equal("apple", t[0].MatchedText(text));
		}


		[Fact]
		public void ShouldMatchTextCaseSensitively()
		{
			const string text = "Apple";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal("apple")));

			try {
				p.Parse(text);
				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Fact]
		public void ShouldMatchTextLenientlyFromOffsetWhenRecovering()
		{
			const string text = "abcdefg";

			var p = new Parser<object>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("bc"),
						new LiteralTerminal("de"),
						new LiteralTerminal("fg"))))
				{ MaxErrors = 3 };

			try {
				p.Parse(text);

				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.Equal(1, e.Context.ErrorCount);
			}
		}


		[Fact]
		public void ShouldNotMatchDifferingText()
		{
			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal("apple")));
			try {
				p.Parse("banana");
				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Fact]
		public void ShouldProduceOneTokenForTheMatch()
		{
			const string text = "abc";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal(text)));
			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal(text, t[0].MatchedText(text));
		}


		[Fact]
		public void ShouldUnescapeLiteralStringExpressions()
		{
			Assert.Equal("They're \\", LiteralTerminal.Unescape("'They\\'re \\\\'"));
		}
	}
}
