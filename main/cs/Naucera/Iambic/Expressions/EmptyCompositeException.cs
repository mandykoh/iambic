namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Exception indicating that an invalid composite expression was
    /// constructed for use.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class EmptyCompositeException : InvalidGrammarException
    {
        private readonly CompositeExpression expression;


        /// <summary>
        /// Creates an EmptyCompositeException for the specified composite.
        /// </summary>
        
        public EmptyCompositeException(CompositeExpression expression)
            : base(expression.GetType().Name)
        {
            this.expression = expression;
        }


        /// <summary>
        /// The composite expression which is empty.
        /// </summary>
        
        public CompositeExpression Expression {
            get { return expression; }
        }
    }
}