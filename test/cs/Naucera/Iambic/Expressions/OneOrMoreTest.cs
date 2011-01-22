using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class OneOrMoreTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new OneOrMore(new LiteralTerminal("a"));

			Assert.AreEqual("'a'+", expr.ToString());
		}


		[Test]
		public void ShouldMatchSubExpressionOnce()
		{
			const string text = "ab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldMatchSubExpressionRepeatedly()
		{
			const string text = "aaab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
					));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldNotLoopForeverIfSubExpressionSuccessfullyMatchesBlank()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(new OneOrMore(new LiteralTerminal(""))),
						new LiteralTerminal("b"))
					));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldNotMatchSubExpressionZeroTimes()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			try {
				p.ParseRaw(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldProduceOneTokenPerTimeMatched()
		{
			const string text = "aaab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			var t = p.ParseRaw(text);

			Assert.AreEqual(4, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("a", t.ChildToken(1).MatchedText(text));
			Assert.AreEqual("a", t.ChildToken(2).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(3).MatchedText(text));
		}


		[Test]
		public void ShouldRecoverFromParseErrorsAtLastRepetition()
		{
			const string text = "xbab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new OneOrMore(
							new Sequence(
								new LiteralTerminal("a"),
								new LiteralTerminal("b"))))
				)) { MaxErrors = 2 };

			try {
				p.ParseRaw(text);
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
				Assert.AreEqual(3, e.Result.ChildCount);
				Assert.AreEqual("b", e.Result.ChildToken(0).MatchedText(text));
				Assert.AreEqual("a", e.Result.ChildToken(1).MatchedText(text));
				Assert.AreEqual("b", e.Result.ChildToken(2).MatchedText(text));
			}
		}
	}
}
