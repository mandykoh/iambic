using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class CustomMatcherLiteralTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new CustomMatcherTerminal("CustomThing");

			Assert.AreEqual("{CustomThing}", expr.ToString());
		}


		[Test]
		public void ShouldInvokeCustomMatcher()
		{
			const string text = "apple";

			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("appleMatcher")),
				new TestCustomMatcher("appleMatcher", "apple"));

			var t = p.ParseRaw(text);

			Assert.AreEqual(0, t.Offset);
			Assert.AreEqual(5, t.EndOffset);
			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("apple", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldInvokeCustomMatcherLenientlyWhenRecovering()
		{
			const string text = "abcdefg";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new CustomMatcherTerminal("bcMatcher"),
						new CustomMatcherTerminal("deMatcher"),
						new CustomMatcherTerminal("fgMatcher"))),
				new TestCustomMatcher("bcMatcher", "bc"),
				new TestCustomMatcher("deMatcher", "de"),
				new TestCustomMatcher("fgMatcher", "fg"))
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
		public void ShouldRejectWhenCustomMatcherDoesNotMatch()
		{
			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("appleMatcher")),
				new TestCustomMatcher("appleMatcher", "apple"));

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

			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("matcher")),
				new TestCustomMatcher("matcher", "abc"));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t.ChildToken(0).MatchedText(text));
		}


		private class TestCustomMatcher : CustomMatcher
		{
			private readonly string toMatch;


			public TestCustomMatcher(string name, string toMatch) : base(name)
			{
				this.toMatch = toMatch;
			}


			public override int Match(string text, int offset)
			{
				if (toMatch.Length <= text.Length - offset && text.IndexOf(toMatch, offset, toMatch.Length) != -1)
					return toMatch.Length;

				return -1;
			}


			public override int MatchLeniently(string text, int offset, out int matchOffset)
			{
				matchOffset = text.IndexOf(toMatch, offset);

				if (matchOffset != -1)
					return toMatch.Length;

				return -1;
			}
		}
	}
}
