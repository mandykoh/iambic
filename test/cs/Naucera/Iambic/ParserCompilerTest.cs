#region license

// Copyright 2011 Amanda Koh. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//    1. Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//
//    2. Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY AMANDA KOH ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL AMANDA KOH OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, either expressed or implied, of Amanda Koh.

#endregion

using System;
using NUnit.Framework;

namespace Naucera.Iambic
{
	[TestFixture]
	public class ParserCompilerTest
	{
		[Test]
		public void ShouldBeAbleToParseOwnPegGrammar()
		{
			var pegGrammar = ParserCompiler.BuildPegGrammarParser<object>().ToString();
			var p = ParserCompiler.Compile<object>(pegGrammar);

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

			Assert.AreEqual(grammar, ParserCompiler.BuildPegGrammarParser<object>().ToString());
		}


		[Test]
		public void ShouldIgnoreBlockCommentsInGrammar()
		{
			var p = ParserCompiler.Compile<object>(
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
			var p = ParserCompiler.Compile<object>(
				"A := B // This is a grammar rule\n" +
				"B := 'abc' // This is another grammar rule");

			p.ParseRaw("abc");

			Assert.AreEqual("A := B" + Environment.NewLine + "B := 'abc'" + Environment.NewLine, p.ToString());
		}


		[Test]
		public void ShouldRejectSpuriousInputAfterGrammar()
		{
			try {
				ParserCompiler.Compile<object>("A := 'abc' :");
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

			var p = ParserCompiler.Compile<object>("A := 'They\\'re \\\\'");
			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("They're \\", t[0].MatchedText(text));
		}


		[Test]
		public void ShouldUnescapeRegexLiteralExpressions()
		{
			const string text = "abc/def\\ghi\\";

			var p = ParserCompiler.Compile<object>("A := /abc\\/def\\\\ghi\\\\/");
			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("abc/def\\ghi\\", t[0].MatchedText(text));
		}
	}
}
