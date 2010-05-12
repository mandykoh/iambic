using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Expression which accepts a string, consuming no input, if its
    /// subexpression also accepts the string at the current position.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class Match : ParseExpression
    {
        private ParseExpression expression;


        /// <summary>
        /// Creates a Match expression for the specified subexpression.
        /// </summary>
        
        public Match(ParseExpression expression)
        {
            this.expression = expression;
        }


        /// <summary>
        /// The subexpression.
        /// </summary>
        
        public ParseExpression Expression {
            get { return expression; }
        }


        internal override bool CheckWellFormed(string baseRuleName,
                                               HashSet<string> ruleNames)
        {
            return expression.CheckWellFormed(baseRuleName, ruleNames);
        }


        /// <summary>
        /// Compiles the child expression, replacing it with the result.
        /// </summary>
        
        internal override ParseExpression Compile(Parser parser)
        {
            expression = expression.Compile(parser);
            return this;
        }


        internal override bool Parse(ParseContext context,
                                     ParseRule rule,
                                     out Token result)
        {
            var offset = context.Offset;
            Token token;

            if (expression.Parse(context, rule, out token)) {
                context.Offset = offset;
                return context.Accept(out result);
            }

            return context.Reject(rule, this, out result);
        }


        public override void ToString(StringBuilder text)
        {
            text.Append('&');
            expression.ToString(text);
        }
    }
}