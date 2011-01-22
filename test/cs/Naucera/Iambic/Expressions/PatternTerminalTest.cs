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
			var p = new Parser<object>(
				new ParseRule("A", new PatternTerminal(".*")));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldMatchTextAtExactOffsetWhenNotRecovering()
		{
			const string text = "abcd";

			var p = new Parser<object>(
				new ParseRule("A", new PatternTerminal("bcd")));

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
						new PatternTerminal("bc"),
						new PatternTerminal("de"),
						new PatternTerminal("fg"))))
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
		public void ShouldMatchTextUsingRegex()
		{
			const string text = "abbccc";

			var p = new Parser<object>(
				new ParseRule("A", new PatternTerminal("a*b*c*d*")));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t.ChildToken(0).MatchedText(text));
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
