using System;
using NUnit.Framework;

namespace Naucera.Iambic
{
	[TestFixture]
	public class ParserFactoryTest
	{
		[Test]
		public void ShouldBeAbleToParseOwnPegGrammar()
		{
			var pegGrammar = ParserFactory.BuildPegGrammarParser<object>().ToString();
			var p = ParserFactory.BuildParser<object>(pegGrammar);

			Assert.AreEqual(pegGrammar, p.ToString());
		}


		[Test]
		public void ShouldBuildPegGrammarParserWithPegGrammar()
		{
			var newLine = Environment.NewLine;

			var grammar =
				@"Grammar := (Ignorable? Definition+ EndOfInput)" + newLine +
				@"Definition := (Identifier ASSIGN Expression)" + newLine +
				@"Expression := (OrderedChoice || Sequence)" + newLine +
				@"OrderedChoice := (Sequence (OR Sequence)+)" + newLine +
				@"Sequence := Prefix+" + newLine +
				@"Prefix := ((AND || NOT)? Suffix)" + newLine +
				@"Suffix := (Primary (QUESTION || STAR || PLUS)?)" + newLine +
				@"Primary := ((Identifier !ASSIGN) || (OPEN Expression CLOSE) || Literal)" + newLine +
				@"Identifier := (/\w+/ Ignorable?)" + newLine +
				@"Literal := (BasicLiteral || RegexLiteral || CustomMatcher)" + newLine +
				@"BasicLiteral := (/'(\\\\|\\'|[^'])*'/ Ignorable?)" + newLine +
				@"RegexLiteral := (/\/(\\\\|\\\/|[^\/])*\// Ignorable?)" + newLine +
				@"CustomMatcher := (/\{\w+\}/ Ignorable?)" + newLine +
				@"EndOfInput := /$/" + newLine +
				@"ASSIGN := (':=' Ignorable?)" + newLine +
				@"OR := ('||' Ignorable?)" + newLine +
				@"AND := ('&' Ignorable?)" + newLine +
				@"NOT := ('!' Ignorable?)" + newLine +
				@"QUESTION := ('?' Ignorable?)" + newLine +
				@"STAR := ('*' Ignorable?)" + newLine +
				@"PLUS := ('+' Ignorable?)" + newLine +
				@"OPEN := ('(' Ignorable?)" + newLine +
				@"CLOSE := (')' Ignorable?)" + newLine +
				@"Ignorable := (Spacing || LineComment || BlockComment)+" + newLine +
				@"Spacing := /\s+/" + newLine +
				@"LineComment := ('//' (!EndOfLine /./)* EndOfLine)" + newLine +
				@"BlockComment := ('/*' (!'*/' /./)* '*/')" + newLine +
				@"EndOfLine := (/$/ || /\r?\n/)" + newLine;

			Assert.AreEqual(grammar, ParserFactory.BuildPegGrammarParser<object>().ToString());
		}


		[Test]
		public void ShouldIgnoreBlockCommentsInGrammar()
		{
			var p = ParserFactory.BuildParser<object>(
				"A := B						/* This is a block comment *\n" +
				"NotARule := NotADefinition  * which spans a few lines *\n" +
				"							 * and includes junk.	   */\n" +
				"B := 'abc'");

			p.ParseRaw("abc");

			Assert.AreEqual("A := B" + Environment.NewLine + "B := 'abc'" + Environment.NewLine, p.ToString());
		}


		[Test]
		public void ShouldIgnoreLineCommentsInGrammar()
		{
			var p = ParserFactory.BuildParser<object>(
				"A := B // This is a grammar rule\n" +
				"B := 'abc' // This is another grammar rule");

			p.ParseRaw("abc");

			Assert.AreEqual("A := B" + Environment.NewLine + "B := 'abc'" + Environment.NewLine, p.ToString());
		}


		[Test]
		public void ShouldRejectSpuriousInputAfterGrammar()
		{
			try {
				ParserFactory.BuildParser<object>("A := 'abc' :");
				Assert.Fail("Invalid grammar was accepted but should have been rejected");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldUnescapeBasicLiteralExpressions()
		{
			const string text = "They're \\";

			var p = ParserFactory.BuildParser<object>("A := 'They\\'re \\\\'");
			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("They're \\", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldUnescapeRegexLiteralExpressions()
		{
			const string text = "abc/def\\ghi\\";

			var p = ParserFactory.BuildParser<object>("A := /abc\\/def\\\\ghi\\\\/");
			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("abc/def\\ghi\\", t.ChildToken(0).MatchedText(text));
		}
	}
}
