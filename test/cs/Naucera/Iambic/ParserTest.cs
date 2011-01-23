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
using Naucera.Iambic.Expressions;
using NUnit.Framework;

namespace Naucera.Iambic
{
	[TestFixture]
	public class ParserTest
	{
		[Test]
		public void ParsingWithConversionShouldReturnConvertedResult()
		{
			var parser = new Parser<int>(new ParseRule("A", new PatternTerminal("[a-zA-Z]+")))
				.Replacing("A", token => 123);

			Assert.AreEqual(123, parser.Parse("sometext"));
		}


		[Test]
		public void ParsingWithConversionShouldPassContextToConverter()
		{
			var parser = new Parser<int>(new ParseRule("A", new PatternTerminal("\\d+")))
				.Replacing("A", (token, ctx) => int.Parse(ctx.MatchedText(token)));

			Assert.AreEqual(123, parser.Parse("123"));
		}


		[Test]
		public void ParsingWithConversionShouldPassArgumentsToConverter()
		{
			var parser = new Parser<int>(new ParseRule("A", new PatternTerminal("\\d+")))
				.Replacing("A", (token, ctx, args) => int.Parse(ctx.MatchedText(token)) + (int)args[0]);

			Assert.AreEqual(128, parser.Parse("123", 5));
		}


		[Test]
		public void ParsingWithoutConversionShouldReturnToken()
		{
			var parser = new Parser<int>(new ParseRule("A", new PatternTerminal("\\d+")));

			var t = parser.ParseRaw("123");

			Assert.AreEqual("123", t.MatchedText("123"));
		}

	
		[Test]
		public void ShouldMapRuleIndicesToRules()
		{
			var ruleA = new ParseRule("RuleA", new LiteralTerminal("a"));
			var ruleB = new ParseRule("RuleB", new LiteralTerminal("b"));

			var parser = new Parser<object>(ruleA, ruleB);

			Assert.AreEqual(ruleA, parser.GetRule(0));
			Assert.AreEqual(ruleB, parser.GetRule(1));
		}


		[Test]
		public void ShouldMapRuleNamesToRules()
		{
			var ruleA = new ParseRule("RuleA", new LiteralTerminal("a"));
			var ruleB = new ParseRule("RuleB", new LiteralTerminal("b"));

			var parser = new Parser<object>(ruleA, ruleB);

			Assert.AreEqual(ruleA, parser.GetRule(ruleA.Name));
			Assert.AreEqual(ruleB, parser.GetRule(ruleB.Name));
		}

		
		[Test]
		public void ShouldNotAllowCustomMatchersWithDuplicateNames()
		{
			try {
				new Parser<object>(
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
				new Parser<object>(
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
		public void ShouldNotAllowParsingWithConversionWhenNoConversionIsDefinedForRootGrammarRule()
		{
			try {
				var p = new Parser<object>(new ParseRule("A", new LiteralTerminal("a")));
				p.Parse("a");

				Assert.Fail("Expected exception was not thrown");
			}
			catch (UndefinedTokenConversionException e) {
				Assert.AreEqual("A", e.Message);
			}
		}


		[Test]
		public void ShouldNotAllowRulesWithDuplicateNames()
		{
			try {
				new Parser<object>(
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
				new Parser<object>(
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
				new Parser<object>();
				Assert.Fail("Expected exception was not thrown");
			}
			catch (EmptyGrammarException) {
				// Expected exception
			}
		}


		class TestCustomMatcher : CustomMatcher
		{
			public TestCustomMatcher(string name) : base(name) {}

			public override int Match(string text, int offset)
			{
				throw new NotImplementedException();
			}
		}
	}
}
