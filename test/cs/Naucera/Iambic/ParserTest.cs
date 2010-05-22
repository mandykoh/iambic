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
        public void ShouldReplaceTokensWithValuesFromProcessors()
        {
            var p = new Parser(
                new ParseRule("A", new Sequence(new RuleRef("B"), new RuleRef("C"), new RuleRef("D")))
                    .ReplacingMatchesWith((token, ctx, args) => new [] { token[0], token[1], token[2] }),

                new ParseRule("B", new LiteralTerminal("b"))
                    .ReplacingMatchesWith((token, ctx, args) => "Matched B"),

                new ParseRule("C", new LiteralTerminal("c"))
                    .ReplacingMatchesWith((token, ctx, args) => "Matched C"),

                new ParseRule("D", new LiteralTerminal("d"))
                    .ReplacingMatchesWith((token, ctx, args) => "Matched D")
            );

            var result = p.Parse("bcd");

            Assert.IsAssignableFrom(typeof(object[]), result);

            var tokens = (object[])result;

            Assert.AreEqual("Matched B", tokens[0]);
            Assert.AreEqual("Matched C", tokens[1]);
            Assert.AreEqual("Matched D", tokens[2]);
        }


        [Test]
        public void ShouldReturnErrorsInOrderOfAppearance()
        {
            var p = new Parser(
                new ParseRule("A", new Sequence(new RuleRef("B"), new RuleRef("C"), new RuleRef("D"), new RuleRef("EOF"))),
                new ParseRule("B", new LiteralTerminal("b")),
                new ParseRule("C", new LiteralTerminal("c")),
                new ParseRule("D", new LiteralTerminal("d")),
                new ParseRule("EOF", new PatternTerminal("$"))
            ) { MaxErrors = 4 };

            try {
                p.Parse("c");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SyntaxException e) {
                Assert.AreEqual(2, e.ErrorCount);
                Assert.AreEqual("'b'", e.GetError(0).Expected.ToString());
                Assert.AreEqual("'d'", e.GetError(1).Expected.ToString());
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
