using Naucera.Iambic.Expressions;
using NUnit.Framework;

namespace Naucera.Iambic
{
    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void ShouldMapRuleIndicesToRules()
        {
            var ruleA = new ParseRule("RuleA", new LiteralTerminal("a"));
            var ruleB = new ParseRule("RuleB", new LiteralTerminal("b"));

            var parser = new Parser(ruleA, ruleB);

            Assert.AreEqual(ruleA, parser.GetRule(0));
            Assert.AreEqual(ruleB, parser.GetRule(1));
        }


        [Test]
        public void ShouldMapRuleNamesToRules()
        {
            var ruleA = new ParseRule("RuleA", new LiteralTerminal("a"));
            var ruleB = new ParseRule("RuleB", new LiteralTerminal("b"));

            var parser = new Parser(ruleA, ruleB);

            Assert.AreEqual(ruleA, parser.GetRule(ruleA.Name));
            Assert.AreEqual(ruleB, parser.GetRule(ruleB.Name));
        }

        
        [Test]
        public void ShouldNotAllowCustomMatchersWithDuplicateNames()
        {
            try {
                new Parser(
                    new ParseRule("RuleA", new CustomMatcherTerminal("CustomMatcher")),
                    new TestCustomMatcher("CustomMatcher"),
                    new TestCustomMatcher("CustomMatcher"));

                Assert.Fail("Expected exception was not thrown");
            }
            catch (DuplicateConstructException e) {
                Assert.AreEqual("CustomMatcher", e.ConstructName);
            }
        }


        [Test]
        public void ShouldNotAllowGrammarConstructsMatchersWithDuplicateNames()
        {
            try {
                new Parser(
                    new ParseRule("Bob", new LiteralTerminal("a")),
                    new ParseRule("Rule", new CustomMatcherTerminal("Bob")),
                    new TestCustomMatcher("Bob"));

                Assert.Fail("Expected exception was not thrown");
            }
            catch (DuplicateConstructException e) {
                Assert.AreEqual("Bob", e.ConstructName);
            }
        }


        [Test]
        public void ShouldNotAllowRulesWithDuplicateNames()
        {
            try {
                new Parser(
                    new ParseRule("RuleA", new LiteralTerminal("a")),
                    new ParseRule("RuleA", new LiteralTerminal("b")));

                Assert.Fail("Expected exception was not thrown");
            }
            catch (DuplicateConstructException e) {
                Assert.AreEqual("RuleA", e.ConstructName);
            }
        }


        [Test]
        public void ShouldNotAllowUndefinedCustomMatchers()
        {
            try {
                new Parser(
                    new ParseRule("Rule", new CustomMatcherTerminal("SomeMatcher")));

                Assert.Fail("Expected exception was not thrown");
            }
            catch (UndefinedConstructException e) {
                Assert.AreEqual("SomeMatcher", e.ConstructName);
            }
        }


        [Test]
        public void ShouldRequireAtLeastOneRule()
        {
            try {
                new Parser();
                Assert.Fail("Expected exception was not thrown");
            }
            catch (EmptyGrammarException) {
                // Expected exception
            }
        }


        private class TestCustomMatcher : CustomMatcher
        {
            public TestCustomMatcher(string name) : base(name) {}
        }
    }
}
