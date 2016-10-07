#region license

// Copyright 2012 Amanda Koh. All rights reserved.
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

using Xunit;

namespace Naucera.Iambic.Expressions
{
	public class RuleRefTest
	{
		[Fact]
		public void ShouldConvertToGrammarString()
		{
			var expr = new RuleRef("OtherRule");

			Assert.Equal("OtherRule", expr.ToString());
		}


		[Fact]
		public void ShouldNotAllowCircularReferences()
		{
			try {
				new Parser<Token>(
					(token, ctx, args) => token,
					new ParseRule("A", new RuleRef("B")),
					new ParseRule("B", new RuleRef("C")),
					new ParseRule("C", new RuleRef("A")));

				Assert.True(false, "Illegal circular reference was accepted but should have been rejected");
			}
			catch (CircularDefinitionException e) {
				Assert.Equal("C", e.BaseRuleName);
				Assert.Equal("A", e.ReferenceName);
			}
		}

		
		[Fact]
		public void ShouldNotAllowReferencingUndefinedRules()
		{
			try {
				new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new RuleRef("B")));
				Assert.True(false, "Undefined reference was accepted but should have been rejected");
			}
			catch (UndefinedConstructException e) {
				Assert.Equal("B", e.ConstructName);
			}
		}


		[Fact]
		public void ShouldParseUsingReferencedRule()
		{
			const string text = "apple";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal("A", t.GrammarConstruct.Name);
			Assert.Equal("B", t[0].GrammarConstruct.Name);
			Assert.Equal(text, t[0][0].MatchedText(text));
		}


		[Fact]
		public void ShouldProduceOneTokenWithOutputFromReferencedRuleAsChildren()
		{
			const string text = "apple";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal(1, t[0].ChildCount);
			Assert.Equal("B", t[0].GrammarConstruct.Name);
			Assert.False(t[0][0].HasChildren);
		}
	}
}
