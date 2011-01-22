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
using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic
{
	/// <summary>
	/// <para>
	/// Parser instance which produces tokens based on expression grammar
	/// rules.</para>
	///
	/// <para>
	/// Instances of this class may be safely used for concurrent parsing by
	/// multiple threads, as long as the parser and its rules are not modified
	/// during parsing.</para>
	/// </summary>

	public sealed class Parser<T>
	{
		public delegate object TokenConversionWithNoArgs(Token token, ParseContext context);

		readonly ParseRule[] mRules;
		readonly Dictionary<string, GrammarConstruct> mConstructNameMap;
		int mMaxErrors = 1;


		/// <summary>
		/// <para>
		/// Creates a Parser that uses the specified grammar constructs.</para>
		///
		/// <para>
		/// At least one grammar rule must be specified.</para>
		/// </summary>
		///
		/// <param name="grammarConstructs">
		/// Parsing grammar constructs.</param>
		///
		/// <exception cref="EmptyGrammarException">
		/// Thrown if no parse rules are specified.</exception>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown on other grammar errors.</exception>
		
		public Parser(params GrammarConstruct[] grammarConstructs)
		{
			// Sort out parse rules
			var parseRules = new List<ParseRule>();
			foreach (var construct in grammarConstructs) {
				if (construct is ParseRule)
					parseRules.Add((ParseRule)construct);
			}

			if (parseRules.Count < 1)
				throw new EmptyGrammarException();

			// Set the grammar rules
			mRules = parseRules.ToArray();

			// Setup the map of grammar constructs by names
			mConstructNameMap = new Dictionary<string, GrammarConstruct>(grammarConstructs.Length);
			foreach (var construct in grammarConstructs) {
				if (mConstructNameMap.ContainsKey(construct.Name))
					throw new DuplicateConstructException(construct.Name);

				mConstructNameMap[construct.Name] = construct;
			}

			// Compile the rules
			foreach (var rule in mRules)
				rule.Compile(this);

			// Check the rules for well-formedness
			foreach (var rule in mRules)
				rule.CheckWellFormed();
		}


		/// <summary>
		/// The maximum number of errors to report while parsing.
		/// </summary>
		
		public int MaxErrors {
			get { return mMaxErrors; }
			set { mMaxErrors = value; }
		}


		/// <summary>
		/// Returns the number of grammar rules defined for this parser.
		/// </summary>
		
		public int RuleCount {
			get { return mRules.Length; }
		}


		/// <summary>
		/// Returns the custom matcher with the specified name, or null if no
		/// such custom matcher exists.
		/// </summary>
		
		internal CustomMatcher CustomMatcher(string matcherName)
		{
			GrammarConstruct matcher;
			mConstructNameMap.TryGetValue(matcherName, out matcher);
			return matcher as CustomMatcher;
		}


		/// <summary>
		/// Returns the grammar rule with the specified index.
		/// </summary>
		
		public ParseRule GetRule(int i)
		{
			return mRules[i];
		}


		/// <summary>
		/// Returns the grammar rule with the specified name, or null if no such
		/// rule has been defined for this parser.
		/// </summary>
		///
		/// <param name="name">
		/// Name of the grammar rule.</param>
		/// 
		/// <returns>Parsing grammar rule, or null.</returns>
		
		public ParseRule GetRule(string name)
		{
			GrammarConstruct rule;
			mConstructNameMap.TryGetValue(name, out rule);
			return rule as ParseRule;
		}


		/// <summary>
		/// Returns the index of the specified rule, or -1 if the rule is not
		/// defined for this parser.
		/// </summary>

		internal int IndexForRule(ParseRule rule)
		{
			for (var i = 0; i < mRules.Length; ++i) {
				if (mRules[i] == rule)
					return i;
			}

			return -1;
		}


		/// <summary>
		/// <para>
		/// Parses the specified text, returning the parsed result as a value by
		/// applying a conversion to the token produced by the root grammar
		/// rule. The conversion should have been registered for the root rule
		/// using Replacing().</para>
		/// 
		/// <para>
		/// Each rule generates a token, with the components of the rule as the
		/// token's children, and each token is converted to a value if a
		/// corresponding conversion has been registered.</para>
		/// </summary>
		///
		/// <param name="text">
		/// Text to parse.</param>
		/// 
		/// <param name="args">
		/// Optional arguments for token conversion.</param>
		/// 
		/// <returns>
		/// Parsed and converted result.</returns>
		///
		/// <exception cref="UndefinedTokenConversionException">
		/// Thrown if no token conversion is defined for the root grammar rule.
		/// </exception>
		/// 
		/// <exception cref="SyntaxException">
		/// Thrown if parsing fails on syntax errors which were unrecoverable
		/// (or MaxErrors was reached).</exception>
		
		public T Parse(string text, params object[] args)
		{
			if (!mRules[0].HasConversion)
				throw new UndefinedTokenConversionException(mRules[0]);

			var context = ParseContext.Create(this, text);
			var resultToken = ParseRawWith(context, text);

			return (T)ReplaceTokens(context, resultToken, args);
		}


		/// <summary>
		/// <para>
		/// Parses the specified text, returning the parsed result as a Token.
		/// No token conversion is performed.</para>
		/// 
		/// <para>
		/// Each rule generates a token, with the components of the rule as the
		/// token's children.</para>
		/// </summary>
		///
		/// <param name="text">
		/// Text to parse.</param>
		/// 
		/// <returns>
		/// Parsed result.</returns>
		///
		/// <exception cref="SyntaxException">
		/// Thrown if parsing fails on syntax errors which were unrecoverable
		/// (or MaxErrors was reached).</exception>
		
		public Token ParseRaw(string text)
		{
			var context = ParseContext.Create(this, text);
			return ParseRawWith(context, text);
		}


		Token ParseRawWith(ParseContext context, string text)
		{
			Token token = null;

			// Parse with the starting rule and recover until we hit our error limit
			for (var i = 0; i < mMaxErrors; ++i) {

				if (mRules[0].Parse(context, out token)) {
					if (context.HasErrors)
						throw new SyntaxException(context, token);

					// Successful parsing - return the token
					return token;
				}

				var error = context.MarkedError;
				context.BeginRecovery();

				// Don't keep parsing if this is the end of the input stream
				if (error.Offset == text.Length)
					break;
			}

			throw new SyntaxException(context, token);
		}


		/// <summary>
		/// Recursively invokes token processors to perform processing on a
		/// token tree after successful parsing. The result of invoking the
		/// processor for the specified token is returned, or the token itself
		/// if no processor is defined for the token's originating parse rule.
		/// </summary>

		static object ReplaceTokens(ParseContext context, Token token, params object[] args)
		{
			// Invoke the processors for the child tokens and replace the children
			// with their results.

			for (int i = 0, c = token.ChildCount; i < c; ++i)
				token[i] = ReplaceTokens(context, token.ChildToken(i), args);

			// Invoke the processor for the current token if it has one
			var origin = token.Origin;
			if (origin != null && origin.HasConversion)
				return origin.ReplaceToken(token, context, args);

			return token;
		}


		/// <summary>
		/// Sets the token conversion for a named grammar construct, which
		/// replaces each occurrence of that construct with a value when Parse()
		/// is invoked.
		/// </summary>
		/// 
		/// <returns>
		/// This parser.</returns>

		public Parser<T> Replacing(string constructName, TokenConversionWithNoArgs with)
		{
			return Replacing(constructName, (token, ctx, args) => with(token, ctx));
		}


		/// <summary>
		/// Sets the token conversion for a named grammar construct, which
		/// replaces each occurrence of that construct with a value using
		/// arguments supplied to Parse().
		/// </summary>
		/// 
		/// <returns>
		/// This parser.</returns>
		
		public Parser<T> Replacing(string constructName, TokenConversion with)
		{
			mConstructNameMap[constructName].ReplacingMatchesWith(with);
			return this;
		}


		/// <summary>
		/// Returns a string containing the grammar describing this parser, as
		/// specified by the ParserFactory documentation.
		/// </summary>

		public override string ToString()
		{
			var newLine = Environment.NewLine;
			var text = new StringBuilder();

			// Stringify the rules
			foreach (var rule in mRules) {
				rule.ToString(text);
				text.Append(newLine);
			}

			return text.ToString();
		}
	}
}
