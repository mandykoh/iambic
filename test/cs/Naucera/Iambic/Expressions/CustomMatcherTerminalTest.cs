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
	public class CustomMatcherLiteralTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new CustomMatcherTerminal("CustomThing");

			Assert.AreEqual("{CustomThing}", expr.ToString());
		}


		[Test]
		public void ShouldInvokeCustomMatcher()
		{
			const string text = "apple";

			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("appleMatcher")),
				new TestCustomMatcher("appleMatcher", "apple"));

			var t = p.ParseRaw(text);

			Assert.AreEqual(0, t.Offset);
			Assert.AreEqual(5, t.EndOffset);
			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("apple", t.ChildToken(0).MatchedText(text));
		}


		[Test]
		public void ShouldInvokeCustomMatcherLenientlyWhenRecovering()
		{
			const string text = "abcdefg";

			var p = new Parser<object>(
				new ParseRule("A",
					new Sequence(
						new CustomMatcherTerminal("bcMatcher"),
						new CustomMatcherTerminal("deMatcher"),
						new CustomMatcherTerminal("fgMatcher"))),
				new TestCustomMatcher("bcMatcher", "bc"),
				new TestCustomMatcher("deMatcher", "de"),
				new TestCustomMatcher("fgMatcher", "fg"))
				{ MaxErrors = 3 };

			try {
				p.ParseRaw(text);

				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.AreEqual(1, e.Context.ErrorCount);
			}
		}


		[Test]
		public void ShouldRejectWhenCustomMatcherDoesNotMatch()
		{
			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("appleMatcher")),
				new TestCustomMatcher("appleMatcher", "apple"));

			try {
				p.ParseRaw("banana");
				Assert.Fail("Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Test]
		public void ShouldProduceOneTokenForTheMatch()
		{
			const string text = "abc";

			var p = new Parser<object>(
				new ParseRule("A", new CustomMatcherTerminal("matcher")),
				new TestCustomMatcher("matcher", "abc"));

			var t = p.ParseRaw(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual(text, t.ChildToken(0).MatchedText(text));
		}


		class TestCustomMatcher : CustomMatcher
		{
			readonly string mToMatch;


			public TestCustomMatcher(string name, string toMatch) : base(name)
			{
				mToMatch = toMatch;
			}


			public override int Match(string text, int offset)
			{
				if (mToMatch.Length <= text.Length - offset && text.IndexOf(mToMatch, offset, mToMatch.Length) != -1)
					return mToMatch.Length;

				return -1;
			}


			public override int MatchLeniently(string text, int offset, out int matchOffset)
			{
				matchOffset = text.IndexOf(mToMatch, offset);

				if (matchOffset != -1)
					return mToMatch.Length;

				return -1;
			}
		}
	}
}
