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

using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class SequenceTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new Sequence(
				new LiteralTerminal("a"),
				new LiteralTerminal("b"));

			// Create a parser to ensure the expression is compiled
			new Parser<object>(new ParseRule("A", expr));

			Assert.AreEqual("('a' 'b')", expr.ToString());
		}


		[Test]
		public void ShouldMatchSubExpressionsInSequence()
		{
			const string text = "ab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			var t = p.ParseRaw(text);

			Assert.AreEqual(2, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(1).MatchedText(text));
		}


		[Test]
		public void ShouldNotAllowInvalidSubExpression()
		{
			try {
				new Parser<object>(
					new ParseRule("A",
						new Sequence(
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
			var expr = new Sequence();

			try {
				new Parser<object>(new ParseRule("A", expr));
				Assert.Fail("Expression without subexpressions was allowed but should have been rejected");
			}
			catch (EmptyCompositeException e) {
				Assert.AreEqual(expr, e.Expression);
			}
		}


		[Test]
		public void ShouldNotMatchSubExpressionsOutOfSequence()
		{
			const string text = "ba";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			try {
				p.ParseRaw(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldProductOneTokenForEachMatchingTerminal()
		{
			const string text = "abc";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))));

			var t = p.ParseRaw(text);

			Assert.AreEqual(3, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(1).MatchedText(text));
			Assert.AreEqual("c", t.ChildToken(2).MatchedText(text));
		}


		[Test]
		public void ShouldRecoverFromParseErrorsAtLastSubExpression()
		{
			const string text = "xbc";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))
				)) { MaxErrors = 3 };

			try {
				p.ParseRaw(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
				Assert.AreEqual(2, e.Result.ChildCount);
				Assert.AreEqual("b", e.Result.ChildToken(0).MatchedText(text));
				Assert.AreEqual("c", e.Result.ChildToken(1).MatchedText(text));
			}
		}
	}
}
