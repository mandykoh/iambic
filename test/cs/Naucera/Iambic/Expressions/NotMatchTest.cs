using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class NotMatchTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new NotMatch(new LiteralTerminal("a"));

			Assert.AreEqual("!'a'", expr.ToString());
		}


		[Test]
		public void ShouldMatchWithoutConsumingInput()
		{
			const string text = "ab";

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
						new NotMatch(new LiteralTerminal("b")),
						new LiteralTerminal("ab"))
				));

			p.Parse(text);
		}


		[Test]
		public void ShouldNotAllowSubexpressionToRecoverFromParseError()
		{
			const string text = "ab";

			var p = new Parser(
				new ParseRule("A", new NotMatch(new LiteralTerminal("a"))))
				{ MaxErrors = 2 };

			try {
				p.Parse(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
				Assert.AreEqual(0, e.Result.ChildCount);
			}
		}


		[Test]
		public void ShouldNotMatchIfSubexpressionMatches()
		{
			const string text = "b";

			var p = new Parser(
				new ParseRule("A", new NotMatch(new LiteralTerminal("b"))));

			try {
				p.Parse(text);
				Assert.Fail("Expression matched when it should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldNotProduceTokenForMatch()
		{
			const string text = "ab";

			var p = new Parser(
				new ParseRule("A",
					new Sequence(
						new NotMatch(new LiteralTerminal("b")),
						new LiteralTerminal("ab"))
				));

			var t = (Token)p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("ab", t.ChildToken(0).MatchedText(text));
		}
	}
}
