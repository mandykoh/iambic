using System.Text;
using Naucera.Iambic.Expressions;
using NUnit.Framework;

namespace Naucera.Iambic
{
    [TestFixture]
    public class TokenTest
    {
        [Test]
        public void ShouldEscapeTextForXml()
        {
            const string text = "<Apple & Pear>";

            var p = new Parser(new ParseRule("A", new LiteralTerminal(text)));
            var t = (Token)p.Parse(text);

            Assert.AreEqual("&lt;Apple &amp; Pear&gt;", t.ChildToken(0).ToXml(text));
        }


        [Test]
        public void ShouldExtractValueFromParsedText()
        {
            const string text = "<Apple & Pear>";

            var p = new Parser(new ParseRule("A", new LiteralTerminal(text)));
            var t = (Token)p.Parse(text);

            Assert.AreEqual("<Apple & Pear>", t.MatchedText(text));
        }


        [Test]
        public void ShouldFormatXmlRespectingSignificantWhitespace()
        {
            const string grammar =
                "Expression := Term Plus Term || Term " +
                "Term := Value '*' Value || Value " +
                "Value := /\\d+/ " +
                "Plus := '+'";
            
            const string text = "1+2*3";

            var expected = new StringBuilder()
                .AppendLine("<Expression>")
                .AppendLine("  <Term>")
                .AppendLine("    <Value>1</Value>")
                .AppendLine("  </Term>")
                .AppendLine("  <Plus>+</Plus>")
                .AppendLine("  <Term><Value>2</Value>*<Value>3</Value></Term>")
                .Append("</Expression>");

            var p = ParserFactory.BuildParser(grammar);
            var t = (Token)p.Parse(text);

            Assert.AreEqual(expected.ToString(), t.ToXml(text));
        }
    }
}
