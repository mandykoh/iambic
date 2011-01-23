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
	/// Parser instance which produces tokens based on expression grammar
	/// rules.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// The first grammar rule specified is always the root grammar rule (also
	/// known as the root or starting production), which represents the overall
	/// entity being parsed.</para>
	///
	/// <para>
	/// Instances of this class may be safely used for concurrent parsing by
	/// multiple threads, as long as the parser and its rules are not modified
	/// during parsing.</para>
	/// </remarks>
	/// 
	/// <typeparam name="T">
	/// Type of the return value. This is the type which the root grammar rule
	/// conversion should return, which is returned by Parse().</typeparam>

	public sealed class Parser<T>
	{
		readonly ParseRule[] mRules;
		readonly Dictionary<string, GrammarConstruct> mConstructNameMap;
		int mMaxErrors = 1;


		/// <summary>
		/// Creates a Parser that uses the specified grammar constructs, which
		/// may be ParseRules or CustomMatchers.
		/// </summary>
		///
		/// <remarks>
		/// At least one ParseRule must be provided, and the first ParseRule is
		/// taken to be the root grammar production.
		/// </remarks>
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
		/// 
		/// <remarks>
		/// <para>
		/// The default is 1 (parsing fails on the first error).</para>
		/// 
		/// <para>
		/// Setting MaxErrors to a higher number will not prevent a
		/// SyntaxException from being thrown; rather, it will cause the parser
		/// to make an attempt to bypass syntax errors while recording them. The
		/// SyntaxException will then contain details of the errors encountered.
		/// </para>
		/// </remarks>
		/// 
		/// <value>
		/// Number of errors to tolerate before a SyntaxException is thrown.
		/// </value>
		
		public int MaxErrors
		{
			get { return mMaxErrors; }
			set { mMaxErrors = value; }
		}


		/// <summary>
		/// The number of grammar rules defined for this parser.
		/// </summary>
		/// 
		/// <value>
		/// Number of ParseRules this parser was created with.</value>
		
		public int RuleCount
		{
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
		/// 
		/// <param name="i">
		/// Zero-based index of the rule, where rules are in the order submitted
		/// during construction.</param>
		/// 
		/// <returns>
		/// Rule with the given index.</returns>
		/// 
		/// <exception cref="IndexOutOfRangeException">
		/// Thrown if no rule with the given index exists.</exception>
		
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
		/// Parses the specified text and converts tokens to user-defined values.
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// Each rule generates a token, with the components of the rule as the
		/// token's children, and each token is converted to a value if a
		/// corresponding conversion has been registered.</para>
		/// 
		/// <para>
		/// The final parsed result is returned by applying a conversion to the
		/// token produced by the root grammar rule. The conversion should have
		/// been registered for the root rule using Replacing().</para>
		/// </remarks>
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
		/// Parses the specified text, returning the parsed result as a Token.
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// Each rule generates a token, with the components of the rule as the
		/// token's children.</para>
		/// 
		/// <para>
		/// No token conversion is performed regardless of any conversions
		/// having been registered using Replacing().</para>
		/// </remarks>
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
			var origin = token.GrammarConstruct;
			if (origin != null && origin.HasConversion)
				return origin.ReplaceToken(token, context, args);

			return token;
		}


		/// <summary>
		/// Registers the token conversion for a named grammar construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each conversion registered in this way results in tokens produced
		/// by the named grammar rule or custom matcher being replaced by the
		/// result of the conversion.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which produced the tokens
		/// to be replaced.</param>
		/// 
		/// <param name="with">
		/// A token conversion which ignores the parsing context and arguments
		/// given to Parse().</param>
		/// 
		/// <returns>
		/// This parser.</returns>

		public Parser<T> Replacing(string constructName, TokenConversionWithNoContext with)
		{
			return Replacing(constructName, (token, ctx, args) => with(token));
		}


		/// <summary>
		/// Registers the token conversion for a named grammar construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each conversion registered in this way results in tokens produced
		/// by the named grammar rule or custom matcher being replaced by the
		/// result of the conversion.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which produced the tokens
		/// to be replaced.</param>
		/// 
		/// <param name="with">
		/// A token conversion which ignores arguments given to Parse().</param>
		/// 
		/// <returns>
		/// This parser.</returns>

		public Parser<T> Replacing(string constructName, TokenConversionWithNoArgs with)
		{
			return Replacing(constructName, (token, ctx, args) => with(token, ctx));
		}


		/// <summary>
		/// Registers the token conversion for a named grammar construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each conversion registered in this way results in tokens produced
		/// by the named grammar rule or custom matcher being replaced by the
		/// result of the conversion.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which produced the tokens
		/// to be replaced.</param>
		/// 
		/// <param name="with">
		/// A token conversion.</param>
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
		/// 
		/// <remarks>
		/// The returned string is in a format suitable for passing to the
		/// ParserCompiler in order to generate a parser matching the same
		/// grammar as this parser.
		/// </remarks>
		/// 
		/// <returns>
		/// PEG grammar specification for the parser's grammar.</returns>

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
