using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class ZeroOrMoreTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new ZeroOrMore(new LiteralTerminal("a"));

			Assert.AreEqual("'a'*", expr.ToString());
		}


		[Test]
		public void ShouldMatchSubExpressionOnce()
		{
			const string text = "ab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
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
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
					));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldMatchSubExpressionZeroTimes()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldNotProduceTokenIfMatchedZeroTimes()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("b", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldProduceOneTokenPerTimeMatched()
		{
			const string text = "aaab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
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
		public void ShouldNotLoopForeverIfSubExpressionSuccessfullyMatchesBlank()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new ZeroOrMore(new LiteralTerminal("a"))),
						new LiteralTerminal("b"))
				));

			p.ParseRaw(text);
		}
	}
}
