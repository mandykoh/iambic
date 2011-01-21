using Naucera.Iambic.Expressions;
using NUnit.Framework;

namespace Naucera.Iambic
{
	[TestFixture]
	public class ParseRuleTest
	{
		[Test]
		public void ShouldInvokeProcessorWithParsedToken()
		{
			const string text = "abc";

			var processorInvoked = false;

			var p = new Parser(
				new ParseRule("A", new LiteralTerminal(text))
					.ReplacingMatchesWith((token, context, args) => {
						Assert.AreEqual(1, token.ChildCount);
						Assert.AreEqual(text, token.ChildToken(0).MatchedText(text));

						processorInvoked = true;
						return null;
					})
				);

			p.Parse(text);

			Assert.IsTrue(processorInvoked);
		}


		[Test]
		public void ShouldReturnOutputFromProcessorIfSpecified()
		{
			var p = new Parser(
				new ParseRule("A", new LiteralTerminal("a"))
					.ReplacingMatchesWith((token, context, args) => "Output From Processor"));

			Assert.AreEqual("Output From Processor", p.Parse("a"));
		}


		[Test]
		public void ShouldReturnTokenWhenNullProcessorIsSpecified()
		{
			var p = new Parser(
				new ParseRule("A", new LiteralTerminal("a"))
					.ReplacingMatchesWith(null));

			Assert.AreEqual(typeof(Token), p.Parse("a").GetType());
		}
	}
}
