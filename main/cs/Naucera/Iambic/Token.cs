using System;
using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic
{
    /// <summary>
    /// A parsed token, which may contain nested tokens.
    /// </summary>
    ///
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>

    public sealed class Token
    {
        readonly int mOffset;
        int mEndOffset;
        GrammarConstruct mOrigin;
        List<object> mChildren;


        internal Token(int offset, int endOffset)
        {
            mOffset = offset;
            mEndOffset = endOffset;
        }


        internal Token(int offset, int endOffset, GrammarConstruct origin)
        {
            mOffset = offset;
            mEndOffset = endOffset;
            mOrigin = origin;
        }


        /// <summary>
        /// Provides access to the child at the specified index.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">
        /// Thrown if there are no children.</exception>

        public object this[int index] {
            get {
                return mChildren[index];
            }

            set {
                mChildren[index] = value;
            }
        }


        public bool Anonymous {
            get { return mOrigin == null; }
        }


        public bool Blank {
            get { return mEndOffset < 0; }
        }


        public int ChildCount {
            get { return mChildren == null ? 0 : mChildren.Count; }
        }


        public int EndOffset {
            get { return mEndOffset; }
            internal set { mEndOffset = value; }
        }


        public bool HasChildren {
            get { return mChildren != null; }
        }


        public int Offset {
            get { return mOffset; }
        }


        public GrammarConstruct Origin {
            get { return mOrigin; }
            internal set { mOrigin = value; }
        }


        internal Token Add(Token token)
        {
            // Unwrap anonymous nested tokens and add their children directly
            if (token.Anonymous && token.HasChildren) {
                if (mChildren == null)
                    mChildren = token.mChildren;
                else
                    mChildren.AddRange(token.mChildren);
            }

            // Otherwise, add the token as a nested child
            else {
                if (mChildren == null)
                    mChildren = new List<object>(4);
                mChildren.Add(token);
            }

            return this;
        }


        /// <summary>
        /// Appends the specified string to a given StringBuilder, escaping
        /// any characters from being interpreted as XML.
        /// </summary>

        static void AppendEscape(StringBuilder buffer, string text)
        {
            for (int i = 0, count = text.Length; i < count; ++i) {
                var c = text[i];

                switch (c) {
                    case '<': buffer.Append("&lt;"); break;
                    case '>': buffer.Append("&gt;"); break;
                    case '&': buffer.Append("&amp;"); break;
                    case '\'': buffer.Append("&apos;"); break;
                    case '"': buffer.Append("&quot;"); break;
                    default: buffer.Append(c); break;
                }
            }
        }


        static void AppendIndent(StringBuilder text, int indentLevel)
        {
            for (var i = 0; i < indentLevel; ++i)
                text.Append("  ");
        }


        bool AllChildrenHaveChildren()
        {
            if (mChildren == null)
                return true;

            foreach (var child in mChildren) {
                var token = child as Token;
                if (token != null && !token.HasChildren)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Returns the child at the specified index as a token.
        /// </summary>
        /// 
        /// <param name="index">
        /// Index of the child.</param>
        /// 
        /// <returns>
        /// Child as a Token.</returns>

        public Token ChildToken(int index)
        {
            return (Token)mChildren[index];
        }


        /// <summary>
        /// Returns true if this token was created by matching the specified
        /// grammar construct name.
        /// </summary>
        
        public bool Matched(string grammarConstructName)
        {
            if (mOrigin == null)
                return grammarConstructName == null;

            return mOrigin.Name == grammarConstructName;
        }


        /// <summary>
        /// Returns the portion of the text which was parsed, as represented by
        /// this token. This is only valid if this token has successfully been
        /// parsed.
        /// </summary>

        public string MatchedText(string originalText)
        {
            return originalText.Substring(mOffset, mEndOffset - mOffset);           
        }


        /// <summary>
        /// Returns the parse tree beginning at this token as an XML string
        /// representation.
        /// </summary>

        public string ToXml(ParseContext context)
        {
            return ToXml(context.BaseText);
        }


        /// <summary>
        /// Returns the parse tree beginning at this token as an XML string
        /// representation.
        /// </summary>

        public string ToXml(string parsedText)
        {
            var text = new StringBuilder();
            ToXml(parsedText, text, true, 0);

            return text.ToString();
        }


        /// <summary>
        /// Appends the parse tree beginning at this token as an XML string to
        /// the specified StringBuilder.
        /// </summary>

        void ToXml(string parsedText, StringBuilder text, bool indent, int indentLevel)
        {
            var hasContent = mEndOffset > mOffset;

            if (indent)
                AppendIndent(text, indentLevel);

            if (mOrigin != null) {
                text.Append('<');
                AppendEscape(text, mOrigin.Name);
                if (!hasContent)
                    text.Append('/');
                text.Append('>');
            }

            indent = indent && AllChildrenHaveChildren();

            if (HasChildren) {
                if (indent)
                    text.AppendLine();

                foreach (var child in mChildren) {
                    var token = child as Token;
                    if (token == null)
                        AppendEscape(text, child.ToString());
                    else
                        token.ToXml(parsedText, text, indent, indentLevel + 1);
                    if (indent)
                        text.AppendLine();
                }
            }
            else if (mEndOffset > mOffset)
                AppendEscape(text, parsedText.Substring(mOffset, mEndOffset - mOffset));

            if (hasContent && mOrigin != null) {
                if (indent)
                    AppendIndent(text, indentLevel);
                text.Append("</");
                AppendEscape(text, mOrigin.Name);
                text.Append('>');
            }
        }
    }
}