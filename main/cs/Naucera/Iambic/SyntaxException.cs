using System;
using System.Collections.Generic;
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
        readonly List<string> mErrorMessages;


        /// <summary>
        /// Creates a SyntaxException with the specified parsing context and
        /// incomplete parse result.
        /// </summary>
        
        internal SyntaxException(ParseContext context, Token result, string sectionSeparator)
        {
            mContext = context;
            mResult = result;
            mErrorMessages = BuildErrorMessages(context, sectionSeparator);
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


        public override string Message {
            get {
                var text = new StringBuilder();

                if (mContext.ErrorCount > 1)
                    text.AppendLine();
                
                foreach (var message in mErrorMessages) {
                    if (mContext.ErrorCount > 1)
                        text.Append(' ');

                    text.AppendLine(message);
                }

                text.Append(base.Message);

                return text.ToString();
            }
        }


        /// <summary>
        /// The result of the parsing.
        /// </summary>
        
        public Token Result {
            get { return mResult; }
        }


        static List<string> BuildErrorMessages(ParseContext context, string sectionSeparator)
        {
            var messages = new List<string>();
            var textLength = context.BaseText.Length;
            var errorLocations = ErrorLocations(context, sectionSeparator);

            for (var i = 0; i < context.ErrorCount; ++i) {
                var error = context.GetError(i);
                var found = context.BaseText.Substring(error.Token.Offset);

                // Truncate the "found" text before the start of the next error
                if (i + 1 < context.ErrorCount) {
                    var nextError = context.GetError(i + 1);
                    var length = nextError.Token.Offset - error.Token.Offset;
                    if (found.Length > length)
                        found = found.Substring(0, length);
                }

                found = found.Replace("\n", "");

                if (found.Length > 0)
                    messages.Add("Expected " + error.Expected + " but found " + Truncate(found) + " when matching " + GetConstructName(error.Token.Origin) + " " + errorLocations[i]);
                else if (error.Token.Offset + found.Length >= textLength)
                    messages.Add("Expected " + error.Expected + " but reached end of input when matching " + GetConstructName(error.Token.Origin) + " " + errorLocations[i]);
                else
                    messages.Add("Expected " + error.Expected + " but was missing when matching " + GetConstructName(error.Token.Origin) + " " + errorLocations[i]);
            }

            return messages;
        }


        static List<string> ErrorLocations(ParseContext context, string sectionSeparator)
        {
            var locations = new List<string>(context.ErrorCount);
            var section = 0;
            var sectionOffset = 0;
            var nextSectionOffset = (sectionSeparator == null ? 0 : -sectionSeparator.Length);

            for (var i = 0; i < context.ErrorCount; ++i) {
                var error = context.GetError(i);

                if (sectionSeparator != null) {
                    while (error.Token.Offset >= nextSectionOffset) {
                        ++section;
                        sectionOffset = nextSectionOffset + sectionSeparator.Length;

                        if (nextSectionOffset + sectionSeparator.Length < context.BaseText.Length) {
                            nextSectionOffset = context.BaseText.IndexOf(sectionSeparator, nextSectionOffset + sectionSeparator.Length);
                            if (nextSectionOffset == -1)
                                nextSectionOffset = context.BaseText.Length + 1;
                        }
                        else
                            nextSectionOffset = context.BaseText.Length + 1;
                    }

                    var column = Math.Max(0, error.Token.Offset - sectionOffset) + 1;
                    locations.Add("(" + section + "," + column + ")");
                }
                else
                    locations.Add("(" + (error.Token.Offset + 1) + ")");
            }

            return locations;
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


        /// <summary>
        /// Returns the parse error message at the specified index.
        /// </summary>
        
        public string GetErrorMessage(int index)
        {
            return mErrorMessages[index];
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