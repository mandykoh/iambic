using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class RuleRefTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new RuleRef("OtherRule");

			Assert.AreEqual("OtherRule", expr.ToString());
		}


		[Test]
		public void ShouldNotAllowCircularReferences()
		{
			try {
				new Parser(
					new ParseRule("A", new RuleRef("B")),
					new ParseRule("B", new RuleRef("C")),
					new ParseRule("C", new RuleRef("A")));

				Assert.Fail("Illegal circular reference was accepted but should have been rejected");
			}
			catch (CircularDefinitionException e) {
				Assert.AreEqual("C", e.BaseRuleName);
				Assert.AreEqual("A", e.ReferenceName);
			}
		}

		
		[Test]
		public void ShouldNotAllowReferencingUndefinedRules()
		{
			try {
				new Parser(new ParseRule("A", new RuleRef("B")));
				Assert.Fail("Undefined reference was accepted but should have been rejected");
			}
			catch (UndefinedConstructException e) {
				Assert.AreEqual("B", e.ConstructName);
			}
		}


		[Test]
		public void ShouldParseUsingReferencedRule()
		{
			const string text = "apple";

			var p = new Parser(
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = (Token)p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("A", t.Origin.Name);
			Assert.AreEqual("B", t.ChildToken(0).Origin.Name);
			Assert.AreEqual(text, t.ChildToken(0).ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldProduceOneTokenWithOutputFromReferencedRuleAsChildren()
		{
			const string text = "apple";

			var p = new Parser(
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = (Token)p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(1, t.ChildToken(0).ChildCount);
			Assert.AreEqual("B", t.ChildToken(0).Origin.Name);
			Assert.IsFalse(t.ChildToken(0).ChildToken(0).HasChildren);
		}
	}
}
