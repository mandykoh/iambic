using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Expression which accepts a string if it matches a regular expression
    /// pattern.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>

    public class PatternTerminal : ParseExpression
    {
        private readonly string pattern;
        private readonly Regex regex;


        /// <summary>
        /// Creates a Terminal expression which matches the specified regular
        /// expression. The regular expression is compiled using the standard
        /// System.Text.RegularExpressions implementation.
        /// </summary>
        /// 
        /// <exception cref="ArgumentException">
        /// Thrown if the regular expression pattern is invalid.</exception>

        public PatternTerminal(string pattern)
        {
            this.pattern = pattern;
            this.regex = new Regex(@"\G" + pattern, RegexOptions.Singleline);
        }


        internal override bool CheckWellFormed(string baseRuleName,
                                               HashSet<string> ruleNames)
        {
            return false;
        }


        private Regex CompensationRegex {
            get { return new Regex(pattern); }
        }


        internal override ParseExpression Compile(Parser parser)
        {
            return this;
        }


        /// <summary>
        /// Escapes the specified string and returns the grammar regex literal
        /// that would match it.
        /// </summary>

        public static string Escape(string text)
        {
            var pattern = new StringBuilder();

            pattern.Append('/');

            for (var i = 0; i < text.Length; ++i) {
                var c = text[i];

                switch (c) {
                    case '/': pattern.Append(@"\/"); break;
                    default: pattern.Append(c); break;
                }
            }

            pattern.Append('/');

            return pattern.ToString();
        }


        internal override bool Parse(ParseContext context,
                                     ParseRule rule,
                                     out Token result)
        {
            if (context.Recovering)
                return context.EndRecovery().Accept(out result);

            // Leniently find the next matching string if we are compensating
            // for an earlier error.
            if (context.Compensating) {
                var match = CompensationRegex.Match(context.BaseText, context.Offset);

                if (match.Success) {
                    context.Offset = match.Index;
                    return context.EndCompensation().Accept(match.Length, out result);
                }

                return context.Accept(out result);
            }

            // Otherwise, look for a precise match at the current position
            else {
                var match = regex.Match(context.BaseText, context.Offset);
                if (match.Success)
                    return context.Accept(match.Length, out result);
            }

            return context.RejectAndMark(rule, this, out result);
        }


        public override void ToString(StringBuilder text)
        {
            text.Append(Escape(pattern));
        }


        /// <summary>
        /// Unescapes the specified grammar regex literal and returns the
        /// literal string that would be matched. The result of calling this on
        /// an invalid grammar regex literal is undefined.
        /// </summary>

        public static string Unescape(string pattern)
        {
            var text = new StringBuilder();

            for (var i = 1; i < pattern.Length - 1; ++i) {
                var c = pattern[i];

                if (c == '\\' && i < pattern.Length - 2) {
                    var next = pattern[i + 1];
                    if (next == '/') {
                        text.Append(next);
                        ++i;
                        continue;
                    }
                }

                text.Append(c);
            }

            return text.ToString();
        }
    }
}