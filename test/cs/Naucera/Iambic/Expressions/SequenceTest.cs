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
			new Parser(new ParseRule("A", expr));

			Assert.AreEqual("('a' 'b')", expr.ToString());
		}


		[Test]
		public void ShouldMatchSubExpressionsInSequence()
		{
			const string text = "ab";

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			var t = (Token)p.Parse(text);

			Assert.AreEqual(2, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(1).MatchedText(text));
		}


		[Test]
		public void ShouldNotAllowInvalidSubExpression()
		{
			try {
				new Parser(
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
				new Parser(new ParseRule("A", expr));
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

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			try {
				p.Parse(text);
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

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))));

			var t = (Token)p.Parse(text);

			Assert.AreEqual(3, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(1).MatchedText(text));
			Assert.AreEqual("c", t.ChildToken(2).MatchedText(text));
		}


		[Test]
		public void ShouldRecoverFromParseErrorsAtLastSubExpression()
		{
			const string text = "xbc";

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))
				)) { MaxErrors = 3 };

			try {
				p.Parse(text);
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
