using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
    /// <summary>
    /// A parsing error.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
        
    public struct ParseError
    {
        readonly Token mToken;
        readonly ParseExpression mExpected;


        internal ParseError(Token token, ParseExpression expected)
        {
            mToken = token;
            mExpected = expected;
        }


        /// <summary>
        /// The expected expression which was not matched.
        /// </summary>
            
        public ParseExpression Expected {
            get { return mExpected; }
        }


        /// <summary>
        /// The parsed token.
        /// </summary>
            
        public Token Token {
            get { return mToken; }
        }
    }
}
