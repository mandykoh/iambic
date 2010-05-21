using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
    [TestFixture]
    public class OrderedChoiceTest
    {
        [Test]
        public void ShouldConvertToGrammarString()
        {
            var expr = new OrderedChoice(
                new LiteralTerminal("a"),
                new LiteralTerminal("b"));

            // Create a parser to ensure the expression is compiled
            new Parser(new ParseRule("A", expr));

            Assert.AreEqual("('a' || 'b')", expr.ToString());
        }


        [Test]
        public void ShouldMarkEntireExpressionAsErrorWhenNoSubExpressionsMatch()
        {
            const string text = "d";

            var orderedChoice = new OrderedChoice(
                new LiteralTerminal("a"),
                new LiteralTerminal("b"),
                new LiteralTerminal("c")
            );

            var p = new Parser(new ParseRule("A", orderedChoice)) { MaxErrors = 3 };

            try {
                p.Parse(text);
                Assert.Fail("Expression matched but should not have");
            }
            catch (SyntaxException e) {
                Assert.AreEqual(1, e.Context.ErrorCount);
                Assert.AreEqual(orderedChoice, e.Context.Expected);
            }
        }


        [Test]
        public void ShouldMatchOneSubExpression()
        {
            var p = new Parser(
                new ParseRule("A",
                    new OrderedChoice(
                      new LiteralTerminal("a"),
                      new LiteralTerminal("b"))));

            var t = (Token)p.Parse("a");

            Assert.AreEqual(1, t.ChildCount);
            Assert.AreEqual("a", t.ChildToken(0).MatchedText("a"));

            t = (Token)p.Parse("b");

            Assert.AreEqual(1, t.ChildCount);
            Assert.AreEqual("b", t.ChildToken(0).MatchedText("b"));
        }


        [Test]
        public void ShouldNotAllowInvalidSubExpression()
        {
            try {
                new Parser(
                    new ParseRule("A",
                        new OrderedChoice(
                            new LiteralTerminal("a"),
                            new RuleRef("nonExistantRule"))));

                Assert.Fail("Invalid subexpression was allowed but should have been rejected");
            }
            catch (UndefinedConstructException e) {
                Assert.AreEqual("nonExistantRule", e.ConstructName);
            }
        }


        [Test]
        public void ShouldNotAllowZeroSubExpressions()
        {
            var expr = new OrderedChoice();

            try {
                new Parser(new ParseRule("A", expr));
                Assert.Fail("Expression without subexpressions was allowed but should have been rejected");
            }
            catch (EmptyCompositeException e) {
                Assert.AreEqual(expr, e.Expression);
            }
        }


        [Test]
        public void ShouldProductOneTokenForOneMatchingTerminal()
        {
            const string text = "abc";

            var p = new Parser(
                new ParseRule("A",
                    new OrderedChoice(
                        new LiteralTerminal("a"),
                        new LiteralTerminal("b"),
                        new LiteralTerminal("c"))));

            var t = (Token)p.Parse(text);

            Assert.AreEqual(1, t.ChildCount);
            Assert.AreEqual("a", t.ChildToken(0).MatchedText("a"));
        }


        [Test]
        public void ShouldRecoverFromParseErrorsAtBestMatchingSubExpression()
        {
            const string text = "bd";

            var p = new Parser(
                new ParseRule("A",
                    new OrderedChoice(
                        new OneOrMore(new LiteralTerminal("a")),
                        new Sequence(new LiteralTerminal("b"), new LiteralTerminal("c")))
                )) { MaxErrors = 3 };

            try {
                p.Parse(text);
                Assert.Fail("Expression matched but should not have");
            }
            catch (SyntaxException e) {
                Assert.AreEqual(1, e.Context.ErrorCount);
                Assert.AreEqual(1, e.Result.ChildCount);
                Assert.AreEqual("b", e.Result.ChildToken(0).MatchedText(text));
            }
        }
    }
}
