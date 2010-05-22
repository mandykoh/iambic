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
    ///
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>

    public sealed class Parser
    {
        readonly ParseRule[] mRules;
        readonly Dictionary<string, GrammarConstruct> mConstructNameMap;
        int mMaxErrors = 1;
        string mSectionSeparator;


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
        /// The number of grammar rules defined for this parser.
        /// </summary>
        
        public int RuleCount {
            get { return mRules.Length; }
        }


        /// <summary>
        /// The separator between text sections, or null if the entire text is
        /// treated as a single section. Examples of sections include lines,
        /// comma separated records, or any other text delimited by a fixed
        /// string.
        /// </summary>
        
        public string SectionSeparator {
            get { return mSectionSeparator; }
            set { mSectionSeparator = value; }
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
        /// Parses the specified text, returning the parsed result as a Token,
        /// or if a TokenProcessorDelegate has been registered with the starting
        /// rule using Replacing(), then the result of invoking the processor is
        /// returned instead.</para>
        /// 
        /// <para>
        /// Each rule generates a token, with the components of the rule as the
        /// token's children.</para>
        /// </summary>
        ///
        /// <param name="text">
        /// Text to parse.</param>
        /// 
        /// <param name="args">
        /// Optional parsing arguments.</param>
        /// 
        /// <returns>
        /// Processed result, or parsed Token if no processor has been
        /// registered for the starting rule.</returns>
        ///
        /// <exception cref="SyntaxException">
        /// Thrown if parsing fails on syntax errors.</exception>
        
        public object Parse(string text, params object[] args)
        {
            var context = new ParseContext(this, text);
            Token token = null;

            // Parse with the starting rule and recover until we hit our error limit
            for (var i = 0; i < mMaxErrors; ++i) {

                if (mRules[0].Parse(context, out token)) {
                    if (context.HasErrors)
                        throw new SyntaxException(context, token, mSectionSeparator);

                    var result = ReplaceTokens(context, token, args);

                    return mRules[0].HasProcessor ? result : token;
                }

                var error = context.MarkedError;
                context.BeginRecovery();

                // Don't keep parsing if this is the end of the input stream
                if (error.Offset == text.Length)
                    break;
            }

            throw new SyntaxException(context, token, mSectionSeparator);
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
            if (origin != null && origin.HasProcessor)
                return origin.ProcessToken(token, context, args);

            return token;
        }


        /// <summary>
        /// Sets the token replacement delegate for a named grammar construct,
        /// which replaces each occurrence of that construct with the return
        /// value of the processor.
        /// </summary>
        /// 
        /// <returns>
        /// This parser.</returns>
        
        public Parser Replacing(string constructName, TokenProcessorDelegate with)
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