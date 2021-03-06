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

using System.Collections.Generic;
using System.Linq;
using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
	/// <summary>
	/// Compiler for conveniently building parsers from a textual grammar.
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// Parsers can be described by Parsing Expression Grammars (PEG) using the
	/// following self-describing grammar specification syntax:</para>
	///
	/// <code>
	///       Grammar := (Ignorable? Definition+ EndOfInput)
	///    Definition := (Identifier ASSIGN Expression)
	///    Expression := (OrderedChoice || Sequence)
	/// OrderedChoice := (Sequence (OR Sequence)+)
	///      Sequence := Prefix+
	///        Prefix := ((AND || NOT)? Suffix)
	///        Suffix := (Primary (QUESTION || STAR || PLUS)?)
	///       Primary := ((Identifier !ASSIGN) || (OPEN Expression CLOSE) || Literal)
	///    Identifier := (/\w+/ Ignorable?)
	///       Literal := (BasicLiteral || RegexLiteral || CustomMatcher)
	///  BasicLiteral := (/'(\\\\|\\'|[^'])*'/ Ignorable?)
	///  RegexLiteral := (/\/(\\\\|\\\/|[^\/])*\// Ignorable?)
	/// CustomMatcher := (/\{\w+\}/ Ignorable?)
	///    EndOfInput := /$/
	///        ASSIGN := (':=' Ignorable?)
	///            OR := ('||' Ignorable?)
	///           AND := ('&amp;' Ignorable?)
	///           NOT := ('!' Ignorable?)
	///      QUESTION := ('?' Ignorable?)
	///          STAR := ('*' Ignorable?)
	///          PLUS := ('+' Ignorable?)
	///          OPEN := ('(' Ignorable?)
	///         CLOSE := (')' Ignorable?)
	///     Ignorable := (Spacing || LineComment || BlockComment)+
	///       Spacing := /\s+/
	///   LineComment := ('//' (!EndOfLine /./)* EndOfLine)
	///  BlockComment := ('/*' (!'*/' /./)* '*/')
	///     EndOfLine := (/\r?\n/ || EndOfInput)
	/// </code>
	///
	/// <para>
	/// The root or starting parse rule is always the first rule specified.
	/// </para>
	///
	/// <para>
	/// Literal text strings inside single quotes (') are treated as literals
	/// matching against the input string exactly. Embedded apostrophes can be
	/// escaped using a backslash, and two backslashes represents a single
	/// backslash.
	/// </para>
	///
	/// <para>
	/// Text strings inside forward slashes ('/') are compiled as
	/// regular expressions using System.Text.RegularExpressions, and match
	/// when the regular expressions match. Embedded forward slashes can be
	/// escaped using a backslash, and two backslashes matches a single
	/// backslash in the text.</para>
	///
	/// <para>
	/// Text inside curly braces ('{' and '}'), which must be alphanumeric or
	/// the underscore character ('_'), are treated as names of custom matchers
	/// to be invoked to match the text. Custom matchers are specified upon
	/// creating a parser, and must be supplied if they are referenced in the
	/// grammar.</para>
	///
	/// <para>
	/// Whitespace is ignored, and rules do not need to be separated by line
	/// breaks.</para>
	///
	/// <para>
	/// Comments may be inserted in the grammar using either single line (//)
	/// style comments (which extend from the // to the end of the line), or
	/// C-style /* */ block comments.</para>
	///
	/// <para>
	/// Parentheses '()' can be used to group parse expressions for
	/// precedence.</para>
	///
	/// <para>
	/// The following grammar constructs are supported:</para>
	///
	/// <list type="table">
	/// <listheader>
	///	 <term>Construct</term>
	///	 <description>Description</description>
	/// </listheader>
	/// <item>
	///	 <term>Sequence: e1 e2</term>
	///	 <description>
	///	 Matches the first expression against the input string, followed
	///	 by the second expression. No input is consumed unless the entire
	///	 sequence matches.</description>
	/// </item>
	/// <item>
	///	 <term>Ordered Choice: e1 || e2</term>
	///	 <descripton>
	///	 Matches the first expression against the input string, then
	///	 attempting to match the second expression only if the first fails
	///	 to match.</descripton>
	/// </item>
	/// <item>
	///	 <term>Optionality: e?</term>
	///	 <description>
	///	 Optionally matches the expression against the input string. No
	///	 token is produced if the expression doesn't match.</description>
	/// </item>
	/// <item>
	///	 <term>Zero-Or-More: e*</term>
	///	 <description>
	///	 Matches the expression against the input string zero or more times.
	///	 No token is produced if the expression doesn't match at least once.
	///	 </description>
	/// </item>
	/// <item>
	///	 <term>One-Or-More: e+</term>
	///	 <description>
	///	 Matches the expression against the input sring one or more times.
	///	 </description>
	/// </item>
	/// <item>
	///	 <term>Match Predicate: &amp;e</term>
	///	 <description>
	///	 Matches the expression against the input string, returning a
	///	 successful match if the expression matches but consuming no input.
	///	 </description>
	/// </item>
	/// <item>
	///	 <term>Not-Match Predicate: !e</term>
	///	 <description>
	///	 Matches the expression against the input string, returning a
	///	 successful match only if the expression doesn't match, and
	///	 consuming no input.</description>
	/// </item>
	/// </list>
	/// </remarks>

	public static class ParserCompiler
	{
		/// <summary>
		/// Creates a parser which parses the specified grammar, as expressed
		/// by the grammar specification language.
		/// </summary>
		/// 
		/// <remarks>
		/// The generated parser returns the root of the parse tree as a token
		/// upon successfully parsing its input. The parser initially will not
		/// have any tagging registered.
		/// </remarks>
		/// 
		/// <param name="grammar">
		/// Textual PEG specification.</param>
		///
		/// <param name="customMatchers">
		/// Custom matchers referenced by the grammar.</param>
		///
		/// <returns>
		/// Parser conforming to the specified grammar.</returns>
		///
		/// <exception cref="SyntaxException">
		/// Thrown if the grammar specification syntax is incorrect.
		/// </exception>
		///
		/// <exception cref="InvalidGrammarException">
		/// Thrown if the grammar contains a problem.</exception>

		public static Parser<Token> Compile(string grammar, params CustomMatcher[] customMatchers)
		{
			var grammarParser = BuildPegGrammarParser();
			return grammarParser.Parse(grammar, customMatchers);
		}


		/// <summary>
		/// Creates a parser which parses the PEG grammar specification
		/// language and generates parsers based on grammar specifications.
		/// </summary>
		///
		/// <remarks>
		/// <para>
		/// This is used internally by Compile() to produce parsers from
		/// grammar specifications.</para>
		/// 
		/// <para>
		/// The resulting parser returns instances of Parser as the result of
		/// parsing a grammar string, in the form described in the
		/// documentation for this class.</para>
		/// </remarks>
		/// 
		/// <returns>
		/// Generated parser.</returns>

		public static Parser<Parser<Token>> BuildPegGrammarParser()
		{
			return new Parser<Parser<Token>>(

				(token, ctx, args) => (Parser<Token>)token.Tag,

				new ParseRule("Grammar",
					new Sequence(
						new Optional(new RuleRef("Ignorable")),
						new OneOrMore(new RuleRef("Definition")),
						new RuleRef("EndOfInput")
						)
					),

				new ParseRule("Definition",
					new Sequence(
						new RuleRef("Identifier"),
						new RuleRef("ASSIGN"),
						new RuleRef("Expression")
						)
					),

				new ParseRule("Expression",
					new OrderedChoice(
						new RuleRef("OrderedChoice"),
						new RuleRef("Sequence")
						)
					),

				new ParseRule("OrderedChoice",
					new Sequence(
						new RuleRef("Sequence"),
						new OneOrMore(
							new Sequence(
								new RuleRef("OR"),
								new RuleRef("Sequence")
								)
							)
						)
					),

				new ParseRule("Sequence",
					new OneOrMore(new RuleRef("Prefix"))
					),

				new ParseRule("Prefix",
					new Sequence(
						new Optional(
							new OrderedChoice(
								new RuleRef("AND"),
								new RuleRef("NOT")
								)
							),
						new RuleRef("Suffix")
						)
					),

				new ParseRule("Suffix",
					new Sequence(
						new RuleRef("Primary"),
						new Optional(
							new OrderedChoice(
								new RuleRef("QUESTION"),
								new RuleRef("STAR"),
								new RuleRef("PLUS")
								)
							)
						)
					),

				new ParseRule("Primary",
					new OrderedChoice(
						new Sequence(
							new RuleRef("Identifier"),
							new NotMatch(new RuleRef("ASSIGN"))
							),
						new Sequence(
							new RuleRef("OPEN"),
							new RuleRef("Expression"),
							new RuleRef("CLOSE")
							),
						new RuleRef("Literal")
						)
					),

				new ParseRule("Identifier",
					new Sequence(
						new PatternTerminal(@"\w+"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("Literal",
					new OrderedChoice(
						new RuleRef("BasicLiteral"),
						new RuleRef("RegexLiteral"),
						new RuleRef("CustomMatcher")
						)
					),

				new ParseRule("BasicLiteral",
					new Sequence(
						new PatternTerminal(@"'(\\\\|\\'|[^'])*'"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("RegexLiteral",
					new Sequence(
						new PatternTerminal(@"/(\\\\|\\/|[^/])*/"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("CustomMatcher",
					new Sequence(
						new PatternTerminal(@"\{\w+\}"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("EndOfInput", new PatternTerminal("$")),

				new ParseRule("ASSIGN",
					new Sequence(
						new LiteralTerminal(":="),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("OR",
					new Sequence(
						new LiteralTerminal("||"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("AND",
					new Sequence(
						new LiteralTerminal("&"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("NOT",
					new Sequence(
						new LiteralTerminal("!"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("QUESTION",
					new Sequence(
						new LiteralTerminal("?"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("STAR",
					new Sequence(
						new LiteralTerminal("*"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("PLUS",
					new Sequence(
						new LiteralTerminal("+"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("OPEN",
					new Sequence(
						new LiteralTerminal("("),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("CLOSE",
					new Sequence(
						new LiteralTerminal(")"),
						new Optional(new RuleRef("Ignorable"))
						)
					),

				new ParseRule("Ignorable",
					new OneOrMore(
						new OrderedChoice(
							new RuleRef("Spacing"),
							new RuleRef("LineComment"),
							new RuleRef("BlockComment")
							)
						)
					),

				new ParseRule("Spacing", new PatternTerminal(@"\s+")),

				new ParseRule("LineComment",
					new Sequence(
						new LiteralTerminal("//"),
						new ZeroOrMore(
							new Sequence(
								new NotMatch(new RuleRef("EndOfLine")),
								new PatternTerminal(".")
								)
							),
						new RuleRef("EndOfLine")
						)
					),

				new ParseRule("BlockComment",
					new Sequence(
						new LiteralTerminal("/*"),
						new ZeroOrMore(
							new Sequence(
								new NotMatch(new LiteralTerminal("*/")),
								new PatternTerminal(".")
								)
							),
						new LiteralTerminal("*/")
						)
					),

				new ParseRule("EndOfLine",
					new OrderedChoice(
						new PatternTerminal("$"),
						new PatternTerminal(@"\r?\n")
						)
					)
				)
				
				.Tagging("Grammar", with: ProcessGrammar)
				.Tagging("Definition", with: ProcessDefinition)
				.Tagging("Expression", with: ProcessExpression)
				.Tagging("OrderedChoice", with: ProcessOrderedChoice)
				.Tagging("Sequence", with: ProcessSequence)
				.Tagging("Prefix", with: ProcessPrefix)
				.Tagging("Suffix", with: ProcessSuffix)
				.Tagging("Primary", with: ProcessPrimary)
				.Tagging("Identifier", with: ProcessIdentifier)
				.Tagging("Literal", with: ProcessLiteral)
				.Tagging("BasicLiteral", with: ProcessBasicLiteral)
				.Tagging("RegexLiteral", with: ProcessRegexLiteral)
				.Tagging("CustomMatcher", with: ProcessCustomMatcher);
		}


		/// <summary>
		/// Returns a LiteralTerminal by processing a literal token.
		/// </summary>

		static object ProcessBasicLiteral(Token token, ParseContext context, object[] args)
		{
			var pattern = context.MatchedText(token[0]);
			pattern = LiteralTerminal.Unescape(pattern);

			return new LiteralTerminal(pattern);
		}


		/// <summary>
		/// Returns a CustomMatcherTerminal by processing a custom matcher
		/// literal token.
		/// </summary>

		static object ProcessCustomMatcher(Token token, ParseContext context, object[] args)
		{
			var matcherName = context.MatchedText(token[0]);
			matcherName = matcherName.Substring(1, matcherName.Length - 2);

			return new CustomMatcherTerminal(matcherName);
		}


		/// <summary>
		/// Returns a ParseRule by processing a definition token.
		/// </summary>

		static object ProcessDefinition(Token token, ParseContext context, object[] args)
		{
			var ruleName = token[0].Tag.ToString();
			var expr = (ParseExpression)token[2].Tag;
			return new ParseRule(ruleName, expr);
		}


		/// <summary>
		/// Returns a ParseExpression by processing an expression token.
		/// </summary>

		static object ProcessExpression(Token token, ParseContext context, object[] args)
		{
			return token[0].Tag;
		}


		/// <summary>
		/// Returns a Parser by processing a grammar token.
		/// </summary>

		static object ProcessGrammar(Token token, ParseContext context, object[] args)
		{
			var grammarConstructs = new List<GrammarConstruct>();

			foreach (var rule in token.Children.Select(c => c.Tag).OfType<ParseRule>())
				grammarConstructs.Add(rule);

			foreach (var arg in args)
				grammarConstructs.Add((CustomMatcher)arg);

			return new Parser<Token>((t, ctx, parseArgs) => t, (ParseRule)grammarConstructs.First(), grammarConstructs.Skip(1).ToArray());
		}


		/// <summary>
		/// Returns an identifier string by processing an identifier token.
		/// </summary>

		static object ProcessIdentifier(Token token, ParseContext context, object[] args)
		{
			return context.MatchedText(token[0]);
		}


		/// <summary>
		/// Returns a terminal expression by processing a literal token.
		/// </summary>

		static object ProcessLiteral(Token token, ParseContext context, object[] args)
		{
			return token[0].Tag;
		}


		/// <summary>
		/// Returns a ParseExpression by processing an ordered choice token.
		/// </summary>

		static object ProcessOrderedChoice(Token token, ParseContext context, object[] args)
		{
			// Form an ordered choice if multiple sequences are present
			if (token.ChildCount > 1) {
				var choiceExpressions = new List<ParseExpression>();

				for (var i = 0; i < token.ChildCount; ++i) {
					var child = token[i].Tag as ParseExpression;
					if (child != null)
						choiceExpressions.Add(child);
				}

				return new OrderedChoice(choiceExpressions.ToArray());
			}

			// Otherwise, just return the first sequence
			return token[0].Tag;
		}


		/// <summary>
		/// Returns a ParseExpression by processing a prefix token.
		/// </summary>

		static object ProcessPrefix(Token token, ParseContext context, object[] args)
		{
			// Wrap the expression with a predicate if a prefix is present
			if (token.ChildCount > 1) {
				var expression = (ParseExpression)token[1].Tag;

				if (token[0].Matched("AND"))
					return new Match(expression);

				return new NotMatch(expression);
			}

			// Otherwise, return the raw expression
			return token[0].Tag;
		}


		/// <summary>
		/// Returns a ParseExpression by processing a primary token.
		/// </summary>

		static object ProcessPrimary(Token token, ParseContext context, object[] args)
		{

			if (token.ChildCount == 1) {
				var firstChild = token[0].Tag;

				// Primary is an identifier - wrap it as a rule reference
				if (firstChild is string)
					return new RuleRef(firstChild.ToString());

				// Primary is a literal - propagate it
				return firstChild;
			}

			// Primary is a nested expression - propagate it
			return token[1].Tag;
		}


		/// <summary>
		/// Returns a PatternTerminal by processing a literal token.
		/// </summary>

		static object ProcessRegexLiteral(Token token, ParseContext context, object[] args)
		{
			var pattern = context.MatchedText(token[0]);
			pattern = PatternTerminal.Unescape(pattern);

			return new PatternTerminal(pattern);
		}


		/// <summary>
		/// Returns a ParseExpression by processing a sequence token.
		/// </summary>

		static object ProcessSequence(Token token, ParseContext context, object[] args)
		{
			// Unwrap sequences of only one nested expression and use as-is
			if (token.ChildCount == 1)
				return token[0].Tag;

			// Otherwise, build a proper sequence from the nested children
			var expressions = new List<ParseExpression>();
			for (var i = 0; i < token.ChildCount; ++i)
				expressions.Add((ParseExpression)token[i].Tag);

			return new Sequence(expressions.ToArray());
		}


		/// <summary>
		/// Returns a ParseExpression by processing a suffix token.
		/// </summary>

		static object ProcessSuffix(Token token, ParseContext context, object[] args)
		{
			var expression = (ParseExpression)token[0].Tag;

			// Wrap the expression if a quantifier is present
			if (token.ChildCount > 1) {
				var quantifier = context.MatchedText(token[1]);

				if (quantifier.StartsWith("?"))
					return new Optional(expression);
				if (quantifier.StartsWith("*"))
					return new ZeroOrMore(expression);

				return new OneOrMore(expression);
			}

			return expression;
		}
	}
}
