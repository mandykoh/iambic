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
				new Parser<object>(
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
				new Parser<object>(new ParseRule("A", new RuleRef("B")));
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

			var p = new Parser<object>(
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("A", t.Origin.Name);
			Assert.AreEqual("B", t.ChildToken(0).Origin.Name);
			Assert.AreEqual(text, t.ChildToken(0).ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldProduceOneTokenWithOutputFromReferencedRuleAsChildren()
		{
			const string text = "apple";

			var p = new Parser<object>(
				new ParseRule("A", new RuleRef("B")),
				new ParseRule("B", new LiteralTerminal(text)));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(1, t.ChildToken(0).ChildCount);
			Assert.AreEqual("B", t.ChildToken(0).Origin.Name);
			Assert.IsFalse(t.ChildToken(0).ChildToken(0).HasChildren);
		}
	}
}
