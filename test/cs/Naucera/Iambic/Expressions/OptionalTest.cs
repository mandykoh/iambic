using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class OptionalTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new Optional(new LiteralTerminal("a"));

			Assert.AreEqual("'a'?", expr.ToString());
		}


		[Test]
		public void ShouldMatchMissingSubExpression()
		{
			const string text = "b";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new Optional(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
					));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldMatchSubExpressionOnce()
		{
			const string text = "ab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new Optional(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			p.ParseRaw(text);
		}


		[Test]
		public void ShouldProduceOneTokenIfSubExpressionMatchedAndNotMissing()
		{
			const string text = "ab";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new Optional(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			var t = p.ParseRaw(text);

			Assert.AreEqual(2, t.ChildCount);
			Assert.AreEqual("a", t.ChildToken(0).MatchedText(text));
			Assert.AreEqual("b", t.ChildToken(1).MatchedText(text));
		}
	}
}
