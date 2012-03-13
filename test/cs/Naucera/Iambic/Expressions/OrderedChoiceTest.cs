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

using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class OrderedChoiceTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new OrderedChoice(
				new LiteralTerminal("a"),
				new LiteralTerminal("b"));

			// Create a parser to ensure the expression is compiled
			new Parser<Token>((token, ctx, args) => token, new ParseRule("A", expr));

			Assert.AreEqual("('a' || 'b')", expr.ToString());
		}


		[Test]
		public void ShouldMatchOneSubExpression()
		{
			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new OrderedChoice(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			var t = p.Parse("a");

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("a", t[0].MatchedText("a"));

			t = p.Parse("b");

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("b", t[0].MatchedText("b"));
		}


		[Test]
		public void ShouldNotAllowInvalidSubExpression()
		{
			try {
				new Parser<Token>(
					(token, ctx, args) => token,
					new ParseRule("A",
						new OrderedChoice(
							new LiteralTerminal("a"),
							new RuleRef("nonExistantRule"))));

				Assert.Fail("Invalid subexpression was allowed but should have been rejected");
			}
			catch (UndefinedConstructException e) {
				Assert.AreEqual("nonExistantRule", e.ConstructName);
			}
		}


		[Test]
		public void ShouldNotAllowZeroSubExpressions()
		{
			var expr = new OrderedChoice();

			try {
				new Parser<Token>((token, ctx, args) => token, new ParseRule("A", expr));
				Assert.Fail("Expression without subexpressions was allowed but should have been rejected");
			}
			catch (EmptyCompositeException e) {
				Assert.AreEqual(expr, e.Expression);
			}
		}


		[Test]
		public void ShouldProductOneTokenForOneMatchingTerminal()
		{
			const string text = "abc";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new OrderedChoice(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))));

			var t = p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("a", t[0].MatchedText("a"));
		}


		[Test]
		public void ShouldRecoverFromParseErrorsAtBestMatchingSubExpression()
		{
			const string text = "bd";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new OrderedChoice(
						new OneOrMore(new LiteralTerminal("a")),
						new Sequence(new LiteralTerminal("b"), new LiteralTerminal("c")))
				)) { MaxErrors = 3 };

			try {
				p.Parse(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
				Assert.AreEqual(1, e.Result.ChildCount);
				Assert.AreEqual("b", e.Result[0].MatchedText(text));
			}
		}
	}
}
