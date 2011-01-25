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
using System.Linq;
using System.Text;
using System.Threading;
using Naucera.Iambic.Expressions;

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
	/// Type of the return value. This is the type which the parser returns
	/// after a successful parsing. Parsers which convert the result to a
	/// particular type can be created using ConvertingResultUsing().
	/// </typeparam>

	public sealed class Parser<T>
	{
		readonly ParseRule mRootRule;
		readonly ParseRule[] mRules;
		readonly Dictionary<string, GrammarConstruct> mConstructNameMap;
		readonly TokenTag[] mTokenTaggers;
		readonly TokenConversion<T> mResultConversion;
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
		/// <param name="resultConversion">
		/// Conversion to apply to the parsed result.</param>
		///
		/// <param name="rootRule">
		/// Root grammar production.</param>
		/// 
		/// <param name="grammarConstructs">
		/// Parsing grammar constructs.</param>
		///
		/// <exception cref="InvalidGrammarException">
		/// Thrown on other grammar errors.</exception>
		
		public Parser(TokenConversion<T> resultConversion, ParseRule rootRule, params GrammarConstruct[] grammarConstructs)
		{
			mResultConversion = resultConversion;

			// Sort out parse rules
			var parseRules = new List<ParseRule>();
			parseRules.Add(rootRule);
			parseRules.AddRange(grammarConstructs.OfType<ParseRule>());

			// Set the grammar rules
			mRootRule = rootRule;
			mRules = parseRules.ToArray();

			// Set up the map of grammar constructs by names
			mConstructNameMap = new Dictionary<string, GrammarConstruct>(grammarConstructs.Length + 1) { { mRootRule.Name, mRootRule } };
			foreach (var construct in grammarConstructs) {
				if (mConstructNameMap.ContainsKey(construct.Name))
					throw new DuplicateConstructException(construct.Name);

				mConstructNameMap[construct.Name] = construct;
			}

			// Compile the rules and assign indices
			for (var i = 0; i < mRules.Length; ++i) {
				mRules[i].Compile(this);
				mRules[i].ParserIndex = i;
			}

			// Check the rules for well-formedness
			foreach (var rule in mRules)
				rule.CheckWellFormed();

			// Assign indices to custom matchers
			var customMatchers = grammarConstructs.OfType<CustomMatcher>().ToArray();
			for (var i = 0; i < customMatchers.Length; ++i)
				customMatchers[i].ParserIndex = mRules.Length + i;

			// Set up table of token taggers for each rule/matcher index
			mTokenTaggers = new TokenTag[mRules.Length + customMatchers.Length];
		}


		Parser(ParseRule rootRule,
			   ParseRule[] rules,
			   Dictionary<string,
			   GrammarConstruct> constructNameMap,
			   IEnumerable<TokenTag> tokenTaggers,
			   TokenConversion<T> resultConversion,
			   int maxErrors)
		{
			mRootRule = rootRule;
			mRules = rules;
			mConstructNameMap = constructNameMap;
			mTokenTaggers = tokenTaggers.ToArray();
			mResultConversion = resultConversion;
			mMaxErrors = maxErrors;
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
		/// Returns a new parser which is a copy of this parser but using the
		/// specified conversion for returning parsed results.
		/// </summary>
		/// 
		/// <typeparam name="TConverted">
		/// Resulting type of the token conversion.</typeparam>
		/// 
		/// <param name="conversion">
		/// The conversion to perform on parsed results.</param>
		/// 
		/// <returns>
		/// New parser.</returns>
		
		public Parser<TConverted> ConvertingResultUsing<TConverted>(TokenConversion<TConverted> conversion)
		{
			return new Parser<TConverted>(mRootRule, mRules, mConstructNameMap, mTokenTaggers, conversion, mMaxErrors);
		}


		/// <summary>
		/// Returns a new parser which is a copy of this parser but using the
		/// specified conversion for returning parsed results.
		/// </summary>
		/// 
		/// <typeparam name="TConverted">
		/// Resulting type of the token conversion.</typeparam>
		/// 
		/// <param name="conversion">
		/// The conversion to perform on parsed results.</param>
		/// 
		/// <returns>
		/// New parser.</returns>
		
		public Parser<TConverted> ConvertingResultUsing<TConverted>(TokenConversionWithNoArgs<TConverted> conversion)
		{
			return ConvertingResultUsing((token, ctx, args) => conversion(token, ctx));
		}


		/// <summary>
		/// Returns a new parser which is a copy of this parser but using the
		/// specified conversion for returning parsed results.
		/// </summary>
		/// 
		/// <typeparam name="TConverted">
		/// Resulting type of the token conversion.</typeparam>
		/// 
		/// <param name="conversion">
		/// The conversion to perform on parsed results.</param>
		/// 
		/// <returns>
		/// New parser.</returns>
		
		public Parser<TConverted> ConvertingResultUsing<TConverted>(TokenConversionWithNoContext<TConverted> conversion)
		{
			return ConvertingResultUsing((token, ctx, args) => conversion(token));
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
		/// Parses the specified text and returns the result.
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// Each rule generates a token, with the components of the rule as the
		/// token's children, and each token is tagged with a value if a
		/// corresponding tag has been registered using Tagging().</para>
		/// 
		/// <para>
		/// The final result is returned by applying a conversion specified upon
		/// construction (or by using ConvertingResultUsing()) to the token
		/// produced by the root grammar rule.</para>
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
		/// <exception cref="SyntaxException">
		/// Thrown if parsing fails on syntax errors which were unrecoverable
		/// (or MaxErrors was reached).</exception>
		
		public T Parse(string text, params object[] args)
		{
			var context = ParseContext.Create(this, text);
			var resultToken = ParseRaw(context, text);

			// Recursively tag tokens
			resultToken = resultToken.WithTag(TagTokens(context, resultToken, args));

			// Convert and return the result
			return mResultConversion(resultToken, context, args);
		}


		Token ParseRaw(ParseContext context, string text)
		{
			var errors = 0;
			Token token;

			// Parse with the starting rule and recover until we hit our error limit
			do {
				if (mRules[0].Parse(context, out token)) {
					if (context.HasErrors)
						throw new SyntaxException(context, token);

					// Successful parsing - return the token
					return token;
				}

				var error = context.MarkedError;
				context.BeginRecovery();

				// Don't keep parsing if this is the end of the input stream
				if (error.Value.Offset == text.Length)
					break;

			} while (++errors < mMaxErrors);

			throw new SyntaxException(context, token);
		}


		/// <summary>
		/// Registers a tag to be applied to tokens matching the named grammar
		/// construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each tag registered in this way causes tokens produced by the named
		/// grammar rule or custom matcher to be tagged with the result of
		/// invoking the tag delegate.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which matches the tokens
		/// to be tagged.</param>
		/// 
		/// <param name="with">
		/// A token tag delegate which takes no arguments.
		/// </param>
		/// 
		/// <returns>
		/// This parser.</returns>
		/// 
		/// <exception cref="UndefinedConstructException">
		/// Thrown if the named construct is not defined for this parser.
		/// </exception>

		public Parser<T> Tagging(string constructName, TokenTagWithNoToken with)
		{
			return Tagging(constructName, (token, ctx, args) => with());
		}


		/// <summary>
		/// Registers a tag to be applied to tokens matching the named grammar
		/// construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each tag registered in this way causes tokens produced by the named
		/// grammar rule or custom matcher to be tagged with the result of
		/// invoking the tag delegate.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which matches the tokens
		/// to be tagged.</param>
		/// 
		/// <param name="with">
		/// A token tag delegate which takes the token being tagged.
		/// </param>
		/// 
		/// <returns>
		/// This parser.</returns>
		/// 
		/// <exception cref="UndefinedConstructException">
		/// Thrown if the named construct is not defined for this parser.
		/// </exception>

		public Parser<T> Tagging(string constructName, TokenTagWithNoContext with)
		{
			return Tagging(constructName, (token, ctx, args) => with(token));
		}


		/// <summary>
		/// Registers a tag to be applied to tokens matching the named grammar
		/// construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each tag registered in this way causes tokens produced by the named
		/// grammar rule or custom matcher to be tagged with the result of
		/// invoking the tag delegate.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which matches the tokens
		/// to be tagged.</param>
		/// 
		/// <param name="with">
		/// A token tag delegate which takes the token being tagged and the
		/// parsing context.
		/// </param>
		/// 
		/// <returns>
		/// This parser.</returns>
		/// 
		/// <exception cref="UndefinedConstructException">
		/// Thrown if the named construct is not defined for this parser.
		/// </exception>

		public Parser<T> Tagging(string constructName, TokenTagWithNoArgs with)
		{
			return Tagging(constructName, (token, ctx, args) => with(token, ctx));
		}


		/// <summary>
		/// Registers a tag to be applied to tokens matching the named grammar
		/// construct.
		/// </summary>
		/// 
		/// <remarks>
		/// Each tag registered in this way causes tokens produced by the named
		/// grammar rule or custom matcher to be tagged with the result of
		/// invoking the tag delegate.
		/// </remarks>
		/// 
		/// <param name="constructName">
		/// Name of the grammar rule or custom matcher which matches the tokens
		/// to be tagged.</param>
		/// 
		/// <param name="with">
		/// A token tag delegate.
		/// </param>
		/// 
		/// <returns>
		/// This parser.</returns>
		/// 
		/// <exception cref="UndefinedConstructException">
		/// Thrown if the named construct is not defined for this parser.
		/// </exception>
		
		public Parser<T> Tagging(string constructName, TokenTag with)
		{
			GrammarConstruct construct;
			if (!mConstructNameMap.TryGetValue(constructName, out construct))
				throw new UndefinedConstructException(constructName);

			mTokenTaggers[construct.ParserIndex] = with;
			return this;
		}


		/// <summary>
		/// Recursively invokes token annotations to annotate a token tree after
		/// successful parsing.
		/// </summary>
		/// 
		/// <remarks>
		/// The result of invoking the annotation for the specified token is
		/// returned, or null if no annotation is defined for the token.
		/// </remarks>

		object TagTokens(ParseContext context, Token token, params object[] args)
		{
			// Invoke the processors for the child tokens and replace the children
			// with their results.

			var childCount = token.ChildCount;
			for (var i = 0; i < childCount; ++i)
				token[i] = token[i].WithTag(TagTokens(context, token[i], args));

			// Invoke the tagger for the current token if it has one
			var origin = token.GrammarConstruct;
			if (origin == null)
				return null;

			var tagger = mTokenTaggers[origin.ParserIndex];
			if (tagger == null)
				return null;

			return tagger(token, context, args);
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
