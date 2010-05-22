using System;
using System.Text;

namespace Naucera.Iambic
{
    /// <summary>
    /// Exception indicating a syntax error encountered during parsing.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class SyntaxException : Exception
    {
        const int MismatchPreviewLength = 32;

        readonly ParseContext mContext;
        readonly Token mResult;


        /// <summary>
        /// Creates a SyntaxException with the specified parsing context and
        /// incomplete parse result.
        /// </summary>
        
        public SyntaxException(ParseContext context, Token result) : base(BuildErrorMessages(context))
        {
            mContext = context;
            mResult = result;
        }


        /// <summary>
        /// The parsing context.
        /// </summary>
        
        public ParseContext Context {
            get { return mContext; }
        }


        /// <summary>
        /// Number of parse errors encountered.
        /// </summary>
        
        public int ErrorCount {
            get { return mContext.ErrorCount; }
        }


        /// <summary>
        /// The result of the parsing.
        /// </summary>
        
        public Token Result {
            get { return mResult; }
        }


        static string BuildErrorMessages(ParseContext context)
        {
            var newLine = Environment.NewLine;
            var text = new StringBuilder();
            var textLength = context.BaseText.Length;

            if (context.ErrorCount > 1)
                text.Append(newLine);

            for (var i = 0; i < context.ErrorCount; ++i) {
                var error = context.GetError(i);

                if (i > 0)
                    text.Append(newLine);

                if (context.ErrorCount > 1)
                    text.Append(' ');

                var found = context.BaseText.Substring(error.Token.Offset);

                // Truncate the "found" text before the start of the next error
                if (i + 1 < context.ErrorCount) {
                    var nextError = context.GetError(i + 1);
                    var length = nextError.Token.Offset - error.Token.Offset;
                    if (found.Length > length)
                        found = found.Substring(0, length);
                }

                if (found.Length > 0)
                    text.Append("Expected " + error.Expected + " but found " + Truncate(found) + " when matching " + GetConstructName(error.Token.Origin));
                else if (error.Token.Offset + found.Length >= textLength)
                    text.Append("Expected " + error.Expected + " but reached end of input when matching " + GetConstructName(error.Token.Origin));
                else
                    text.Append("Expected " + error.Expected + " but was missing when matching " + GetConstructName(error.Token.Origin));
            }

            return text.ToString();
        }


        static string GetConstructName(GrammarConstruct construct)
        {
            return construct == null ? "null" : construct.Name;
        }


        /// <summary>
        /// Returns the parse error at the specified index.
        /// </summary>
        
        public ParseError GetError(int index)
        {
            return mContext.GetError(index);
        }


        static string Truncate(string text)
        {
            if (text == null)
                return null;

            if (text.Length > MismatchPreviewLength)
                return text.Substring(0, MismatchPreviewLength) + "...";

            return text;
        }
    }
}