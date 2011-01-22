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

using System.Collections.Generic;
using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
	/// <summary>
	/// Context for holding a parsing state and generating tokens for matches
	/// based on that state.
	/// </summary>
	///
	/// <remarks>
	/// Instances of this class are not thread-safe, and are created for each
	/// invocation of a Parser on a body of text.
	/// </remarks>

	public sealed class ParseContext
	{
		static readonly Dictionary<int, CacheEntry> EmptyRuleCache = new Dictionary<int, CacheEntry>();

		readonly Dictionary<int, CacheEntry>[] mRuleCaches;
		readonly List<Token> mErrors = new List<Token>();
		readonly string mBaseText;
		readonly bool mErrorRecoveryEnabled;
		int mOffset;
		readonly List<ParseExpression.Memento> mExpressionStateStack = new List<ParseExpression.Memento>();
		List<ParseExpression.Memento> mErrorStateStack;
		Token mErrorStateToken;
		ParseExpression mExpected;
		int mErrorStackIndex = -1;
		bool mCompensating;


		ParseContext(bool errorRecoveryEnabled, int ruleCount, string text)
		{
			mBaseText = text;
			mErrorRecoveryEnabled = errorRecoveryEnabled;

			// Set up the rule caches
			mRuleCaches = new Dictionary<int, CacheEntry>[ruleCount];
			for (var i = 0; i < mRuleCaches.Length; ++i)
				mRuleCaches[i] = EmptyRuleCache;
		}


		/// <summary>
		/// The original source text being parsed.
		/// </summary>
		
		public string BaseText
		{
			get { return mBaseText; }
		}


		/// <summary>
		/// Flag indicating if error compensation is in progress.
		/// </summary>

		internal bool Compensating
		{
			get { return mCompensating; }
		}


		/// <summary>
		/// The number of parse errors.
		/// </summary>
		
		public int ErrorCount
		{
			get { return mErrors.Count; }
		}


		/// <summary>
		/// The expected expression, if a parsing error was encountered.
		/// </summary>

		public ParseExpression Expected
		{
			get { return mExpected; }
		}


		/// <summary>
		/// Flag indicating if any parse errors have been encountered.
		/// </summary>
		
		public bool HasErrors
		{
			get { return mErrors.Count > 0; }
		}


		/// <summary>
		/// The furthest error token encountered so far, or null if none have
		/// been encountered.
		/// </summary>

		internal Token MarkedError
		{
			get { return mErrorStateToken; }
		}


		/// <summary>
		/// The current parsing offset in the source text.
		/// </summary>
		/// 
		/// <value>
		/// Zero-based offset in characters.</value>
		
		public int Offset
		{
			get { return mOffset; }
			internal set { mOffset = value; }
		}


		/// <summary>
		/// Flag indicating if error recovery is in progress.
		/// </summary>

		internal bool Recovering
		{
			get { return mErrorStackIndex != -1; }
		}


		/// <summary>
		/// Creates a blank, anonymous token accepting the text at the current
		/// position.
		/// </summary>
		/// 
		/// <returns>
		/// Always true.</returns>

		internal bool Accept(out Token result)
		{
			result = new Token(mOffset, -1);
			return true;
		}


		/// <summary>
		/// Creates a blank, anonymous token accepting the text at the current
		/// position, which was matched by the specified grammar construct.
		/// </summary>
		/// 
		/// <returns>
		/// Always true.</returns>

		internal bool Accept(GrammarConstruct origin, out Token result)
		{
			result = new Token(mOffset, -1, origin);
			return true;
		}


		/// <summary>
		/// Creates an anonymous token accepting the text up to the specified
		/// number of characters at the current position. The current position
		/// is advanced by the length of the accepted text.
		/// </summary>
		/// 
		/// <returns>
		/// Always true.</returns>

		internal bool Accept(int length, out Token result)
		{
			var tmp = mOffset + length;
			result = new Token(mOffset, tmp);
			mOffset = tmp;

			return true;
		}


		/// <summary>
		/// Creates an anonymous token accepting the text up to the specified
		/// number of characters at the current position. The current position
		/// is advanced by the length of the accepted text.
		/// </summary>
		/// 
		/// <returns>
		/// Always true.</returns>

		internal bool Accept(int length, GrammarConstruct origin, out Token result)
		{
			var tmp = mOffset + length;
			result = new Token(mOffset, tmp, origin);
			mOffset = tmp;

			return true;
		}


		/// <summary>
		/// Marks the beginning of a stateful expression by pushing a memento
		/// containing its state onto the expression state stack.
		/// </summary>
		/// 
		/// <returns>This context.</returns>
		
		internal ParseContext BeginExpression(ParseExpression.Memento memento)
		{
			mExpressionStateStack.Add(memento);
			return this;
		}


		/// <summary>
		/// Sets the error recovery state and error compensation flag, and marks
		/// the last encountered rejection as a parse error.
		/// </summary>
		///
		/// <returns>This context.</returns>

		internal ParseContext BeginRecovery()
		{
			mErrors.Add(mErrorStateToken);

			mCompensating = true;
			mOffset = mErrorStateToken.Offset;

			mErrorStateToken = null;
			mErrorStackIndex = 0;

			return this;
		}


		/// <summary>
		/// <para>
		/// Caches the result of parsing using the specified rule at the given
		/// offset in the text. When the result is retrieved using the
		/// UseCachedResult() method, the current parsing position is restored
		/// as well.</para>
		///
		/// <para>
		/// This should only be called with a rule in the parser for which this
		/// context was created.</para>
		/// </summary>
		///
		/// <param name="rule">
		/// The rule which was applied to arrive at the parsed result.</param>
		/// 
		/// <param name="ruleOffset">
		/// Text offset at which the rule was applied.</param>
		/// 
		/// <param name="accepted">
		/// Whether the result was accepted or rejected.</param>
		/// 
		/// <param name="result">
		/// Parsing result to cache for future retrieval.</param>
		/// 
		/// <returns>This context.</returns>

		internal ParseContext CacheResult(ParseRule rule, int ruleOffset, bool accepted, Token result)
		{
			// Create a cache for this rule and stop using the empty one
			if (ReferenceEquals(mRuleCaches[rule.Index], EmptyRuleCache))
				mRuleCaches[rule.Index] = new Dictionary<int, CacheEntry>();

			var entry = new CacheEntry(mOffset, accepted, result);
			mRuleCaches[rule.Index][ruleOffset] = entry;
			return this;
		}


		/// <summary>
		/// Clears the error recovery status.
		/// </summary>
		/// 
		/// <returns>This context.</returns>
		
		internal ParseContext ClearRecovery()
		{
			mErrorStateToken = null;
			mErrorStackIndex = -1;
			return this;
		}


		/// <summary>
		/// Creates a ParseContext for the specified parser and text.
		/// </summary>

		internal static ParseContext Create<T>(Parser<T> parser, string text)
		{
			return new ParseContext(
				parser.MaxErrors > 1,
				parser.RuleCount,
				text);
		}


		/// <summary>
		/// Clears the error compensation state.
		/// </summary>
		/// 
		/// <returns>This context.</returns>
		
		internal ParseContext EndCompensation()
		{
			mCompensating = false;
			return this;
		}


		/// <summary>
		/// Marks the end of a stateful expression by popping a memento
		/// containing its state (pushed by the beginExpression() method) from
		/// the expression state stack.
		/// </summary>
		/// 
		/// <returns>This context.</returns>
		
		internal ParseContext EndExpression()
		{
			mExpressionStateStack.RemoveAt(mExpressionStateStack.Count - 1);
			return this;
		}


		/// <summary>
		/// Clears the error recovery state.
		/// </summary>
		/// 
		/// <returns>This context.</returns>
		
		internal ParseContext EndRecovery()
		{
			mErrorStackIndex = -1;
			return this;
		}


		/// <summary>
		/// Returns the error with the specified index.
		/// </summary>
		/// 
		/// <param name="i">
		/// Index of the error.</param>
		
		public Token GetError(int i)
		{
			return mErrors[i];
		}


		/// <summary>
		/// Returns the portion of the base text being parsed, as represented by
		/// the given token.
		/// </summary>
		/// 
		/// <remarks>
		/// This is only valid for tokens which have successfully
		/// been parsed.
		/// </remarks>
		/// 
		/// <param name="token">
		/// Token for which to find the matched text.</param>
		/// 
		/// <returns>
		/// Text matched by the token (including its children).</returns>
		
		public string MatchedText(Token token)
		{
			return token.MatchedText(mBaseText);
		}


		/// <summary>
		/// Returns a stateful expression memento from the expression state
		/// stack as it was at the time of the error (the last call to
		/// RejectAndMark()).
		/// </summary>
		
		internal ParseExpression.Memento Recover()
		{
			return mErrorStateStack[mErrorStackIndex++];
		}


		/// <summary>
		/// Creates an anonymous token representing a rejection of the text at
		/// the current position, by the given rule and expression.
		/// </summary>
		/// 
		/// <returns>
		/// Always false.</returns>

		internal bool Reject(ParseRule rule, ParseExpression expression, out Token result)
		{
			mExpected = expression;
			result = new Token(mOffset, mBaseText.Length - 1, rule);
			return false;
		}


		/// <summary>
		/// Creates an anonymous token representing a rejection of the text at
		/// the current position, by the given rule and expression. The
		/// rejection is marked as a potentially recoverable parse error.
		/// </summary>
		/// 
		/// <returns>
		/// Always false.</returns>

		internal bool RejectAndMark(ParseRule rule, ParseExpression expression, out Token result)
		{
			Reject(rule, expression, out result);

			// If the rejection was further in the text than the last error,
			// mark it as the "best error" found so far.
			if (mErrorStateToken == null || mErrorStateToken.Offset < mOffset) {
				mErrorStateToken = result;

				// Copy the expression state stack to use as the errored state
				if (mErrorRecoveryEnabled) {
					mErrorStateStack = new List<ParseExpression.Memento>(mExpressionStateStack.Count);
					for (var i = 0; i < mExpressionStateStack.Count; ++i) {
						var m = mExpressionStateStack[i];
						mErrorStateStack.Add(m.Copy());
					}
				}
			}

			return false;
		}


		/// <summary>
		/// <para>
		/// Returns a cached result for the specified rule at the current
		/// parsing position, setting the parsing position to the offset as
		/// determined by the initial parse.</para>
		///
		/// <para>
		/// If no cached result exists, null is returned instead.</para>
		///
		/// <para>
		/// This should only be called with a rule in the parser for which this
		/// context was created.</para>
		/// </summary>
		/// 
		/// <param name="rule">
		/// Rule to retrieve a cached result for.</param>
		/// 
		/// <returns>Cached parsing result, or null.</returns>

		internal CacheEntry UseCachedResult(ParseRule rule)
		{
			var index = rule.Index;

			CacheEntry entry;
			if (!mRuleCaches[index].TryGetValue(mOffset, out entry))
				return null;

			mOffset = entry.Offset;

			return entry;
		}


		/// <summary>
		/// Internal entry for a cached parse result of a rule.
		/// </summary>

		internal class CacheEntry
		{
			internal readonly int Offset;
			internal readonly bool Accepted;
			internal readonly Token Result;


			public CacheEntry(int offset, bool accepted, Token result)
			{
				Offset = offset;
				Accepted = accepted;
				Result = result;
			}
		}
	}
}
