using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
    [TestFixture]
    public class MatchTest
    {
        [Test]
        public void ShouldConvertToGrammarString()
        {
            var expr = new Match(new LiteralTerminal("a"));

            Assert.AreEqual("&'a'", expr.ToString());
        }


        [Test]
        public void ShouldMatchSubExpressionWithoutConsumingInput()
        {
            const string text = "ab";

            var p = new Parser(
                new ParseRule("A",
                    new Sequence(
                        new Match(new LiteralTerminal("a")),
                        new LiteralTerminal("ab"))
                ));

            p.Parse(text);
        }


        [Test]
        public void ShouldNotMatchIfSubexpressionDoesNotMatch()
        {
            const string text = "b";

            var p = new Parser(
                new ParseRule("A",
                    new Sequence(
                        new Match(new LiteralTerminal("a")),
                        new LiteralTerminal("b"))
                ));

            try {
                p.Parse(text);
                Assert.Fail("Expression matched when it should not have");
            }
            catch (SyntaxException) {
                // Expected exception
            }
        }


        [Test]
        public void ShouldNotProduceTokenForMatch()
        {
            const string text = "ab";

            var p = new Parser(
                new ParseRule("A",
                    new Sequence(
                        new Match(new LiteralTerminal("a")),
                        new LiteralTerminal("ab"))
                ));

            var t = (Token)p.Parse(text);

            Assert.AreEqual(1, t.ChildCount);
            Assert.AreEqual("ab", t.ChildToken(0).MatchedText(text));
        }
    }
}
