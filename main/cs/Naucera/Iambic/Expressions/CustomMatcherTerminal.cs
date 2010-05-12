using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Expression which accepts a string by invoking a custom matcher.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>

    public class CustomMatcherTerminal : ParseExpression
    {
        private readonly string matcherName;
        private CustomMatcher customMatcher;


        /// <summary>
        /// Creates a CustomMatcherTerminal with the specified name. The name
        /// is used to identify a custom matcher referenced in a parser grammar.
        /// </summary>
        
        public CustomMatcherTerminal(string matcherName)
        {
            this.matcherName = matcherName;
        }


        internal override bool CheckWellFormed(string baseRuleName,
                                               HashSet<string> ruleNames)
        {
            return false;
        }


        internal override ParseExpression Compile(Parser parser)
        {
            customMatcher = parser.CustomMatcher(matcherName);
            if (customMatcher == null)
                throw new UndefinedConstructException(matcherName);

            return this;
        }


        internal override bool Parse(ParseContext context,
                                     ParseRule rule,
                                     out Token result)
        {
            if (context.Recovering)
                return context.EndRecovery().Accept(customMatcher, out result);

            // Leniently find the next matching string if we are compensating
            // for an earlier error.
            if (context.Compensating) {
                int offset;
                var length = customMatcher.MatchLeniently(context.BaseText, context.Offset, out offset);

                if (length >= 0) {
                    context.Offset = offset;
                    return context.EndCompensation().Accept(length, customMatcher, out result);
                }

                return context.Accept(customMatcher, out result);
            }

            // Otherwise, look for a precise match at the current position
            else {
                var length = customMatcher.Match(context.BaseText, context.Offset);

                if (length >= 0)
                    return context.Accept(length, customMatcher, out result);
            }

            return context.RejectAndMark(rule, this, out result);
        }


        public override void ToString(StringBuilder text)
        {
            text.Append("{").Append(matcherName).Append("}");
        }
    }
}