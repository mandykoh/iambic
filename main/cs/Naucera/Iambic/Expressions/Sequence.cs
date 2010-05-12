using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Composite expression which evaluates a sequence of nested expressions,
    /// accepting a string if it matches each expression in turn, and rejecting
    /// it otherwise.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class Sequence : CompositeExpression
    {
        /// <summary>
        /// Creates a Sequence from the specified sub-expressions.
        /// </summary>

        public Sequence(params ParseExpression[] expressions)
            : base(expressions)
        {
        }


        internal override bool CheckWellFormed(string baseRuleName,
                                               HashSet<string> ruleNames)
        {
            for (var i = 0; i < ExpressionCount; ++i) {
                var tmp = new HashSet<string>(ruleNames);
                if (!Expression(i).CheckWellFormed(baseRuleName, tmp)) {
                    foreach (var name in tmp)
                        ruleNames.Add(name);
                    return false;
                }
            }

            return true;
        }


        internal override bool Parse(ParseContext context,
                                     ParseRule rule,
                                     out Token result)
        {
            SequenceMemento state;
            if (context.Recovering)
                state = (SequenceMemento)context.Recover();
            else
                state = new SequenceMemento(context);

            context.BeginExpression(state);

            // Parse each subexpression to find nested tokens
            for (; state.index < ExpressionCount; ++state.index) {
                var expr = Expression(state.index);
                Token t;

                // Parsing failed on a subexpression - return an overall failure
                if (!expr.Parse(context, rule, out t)) {
                    context.Offset = state.offset;
                    context.EndExpression();
                    result = t;
                    return false;
                }

                // Skip blank tokens
                if (t.Blank)
                    continue;

                // Add the parsed token to our result
                state.result.Add(t);
            }

            if (state.result.HasChildren)
                state.result.EndOffset = context.Offset;

            context.EndExpression();

            result = state.result;
            return true;
        }


        public override void ToString(StringBuilder text)
        {
            text.Append('(');

            for (var i = 0; i < ExpressionCount; ++i) {
                var expr = Expression(i);
                if (i > 0)
                    text.Append(' ');
                expr.ToString(text);
            }

            text.Append(')');
        }


        private sealed class SequenceMemento : Memento
        {
            internal readonly Token result;
            internal int index;
            internal readonly int offset;


            public SequenceMemento(ParseContext context)
            {
                context.Accept(out result);
                offset = context.Offset;
            }


            private SequenceMemento(SequenceMemento m)
            {
                this.result = m.result;
                this.index = m.index;
                this.offset = m.offset;
            }


            internal override Memento Copy()
            {
                return new SequenceMemento(this);
            }
        }
    }
}