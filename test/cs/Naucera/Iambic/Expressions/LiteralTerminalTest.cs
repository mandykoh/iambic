using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class LiteralTerminalTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new LiteralTerminal("D'Angelo");

			Assert.AreEqual("'D\\'Angelo'", expr.ToString());
		}


		[Test]
		public void ShouldEscapeLiteralStringExpressions()
		{
			Assert.AreEqual("'They\\'re \\\\'", LiteralTerminal.Escape("They're \\"));
		}


		[Test]
		public void ShouldMatchPrefixOfText()
		{
			const string text = "applepie";

			var p = new Parser<object>(new ParseRule("A", new LiteralTerminal("apple")));
			var t = p.ParseRaw(text);

			Assert.AreEqual(0, t.Offset);
			Assert.AreEqual(5, t.EndOffset);
			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("apple", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldMatchTextAtExactOffsetWhenNotRecovering()
		{
			const string text = "apple";

			var p = new Parser<object>(new ParseRule("A", new LiteralTerminal("apple")));
			var t = p.ParseRaw(text);

			Assert.AreEqual(0, t.Offset);
			Assert.AreEqual(5, t.EndOffset);
			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("apple", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldMatchTextCaseSensitively()
		{
			const string text = "Apple";

			var p = new Parser<object>(new ParseRule("A", new LiteralTerminal("apple")));

			try {
				p.ParseRaw(text);
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

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("bc"),
						new LiteralTerminal("de"),
						new LiteralTerminal("fg"))))
				{ MaxErrors = 3 };

			try {
				p.ParseRaw(text);

				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
			}
		}


		[Test]
		public void ShouldNotMatchDifferingText()
		{
			var p = new Parser<object>(new ParseRule("A", new LiteralTerminal("apple")));
			try {
				p.ParseRaw("banana");
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldProduceOneTokenForTheMatch()
		{
			const string text = "abc";

			var p = new Parser<object>(new ParseRule("A", new LiteralTerminal(text)));
			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldUnescapeLiteralStringExpressions()
		{
			Assert.AreEqual("They're \\", LiteralTerminal.Unescape("'They\\'re \\\\'"));
		}
	}
}
