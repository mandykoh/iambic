#region license

// Copyright 2011 Amanda Koh. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//    1. Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//
//    2. Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY AMANDA KOH ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL AMANDA KOH OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, either expressed or implied, of Amanda Koh.

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic
{
	/// <summary>
	/// A parsed token representing a node/subtree of a parse tree.
	/// </summary>
	/// 
	/// <remarks>
	/// Tokens may contain nested children which are either also tokens, or
	/// values which have replaced their tokens due to conversion.
	/// </remarks>

	public sealed class Token
	{
		readonly int mOffset;
		int mEndOffset;
		GrammarConstruct mGrammarConstruct;
		List<object> mChildren;


		internal Token(int offset, int endOffset)
		{
			mOffset = offset;
			mEndOffset = endOffset;
		}


		internal Token(int offset, int endOffset, GrammarConstruct grammarConstruct)
		{
			mOffset = offset;
			mEndOffset = endOffset;
			mGrammarConstruct = grammarConstruct;
		}


		/// <summary>
		/// Provides access to the child at the specified index.
		/// </summary>
		/// 
		/// <remarks>
		/// The child may be a Token or a value from a conversion.
		/// </remarks>
		/// 
		/// <param name="index">
		/// Zero-based index of the child.</param>
		/// 
		/// <value>
		/// A token if the child has not been replaced by conversion, or the
		/// converted value if it has.</value>
		/// 
		/// <exception cref="NullReferenceException">
		/// Thrown if there are no children.</exception>

		public object this[int index]
		{
			get { return mChildren[index]; }
			internal set { mChildren[index] = value; }
		}


		/// <summary>
		/// Flag indicating whether this token is anonymous.
		/// </summary>
		/// 
		/// <remarks>
		/// Anonymous tokens do not represent a match of an entire grammar
		/// construct, and are produced as a result of evaluating portions of a
		/// grammar rule.
		/// </remarks>
		/// 
		/// <value>
		/// True if anonymous, false otherwise.</value>
		
		public bool Anonymous
		{
			get { return mGrammarConstruct == null; }
		}


		/// <summary>
		/// Flag indicating whether this token is blank, ie. matches an empty
		/// string.
		/// </summary>
		/// 
		/// <value>
		/// True if blank, false otherwise.</value>
		
		public bool Blank
		{
			get { return mEndOffset < 0; }
		}


		/// <summary>
		/// The number of children this token has.
		/// </summary>
		
		public int ChildCount
		{
			get { return mChildren == null ? 0 : mChildren.Count; }
		}


		/// <summary>
		/// The end offset in the parsed text matched by this token.
		/// </summary>
		/// 
		/// <value>
		/// Zero-based offset in characters from start of text.</value>
		
		public int EndOffset
		{
			get { return mEndOffset; }
			internal set { mEndOffset = value; }
		}


		/// <summary>
		/// Flag indicating whether this token has any children.
		/// </summary>
		/// 
		/// <value>
		/// True if there is at least one child, false otherwise.</value>
		
		public bool HasChildren
		{
			get { return mChildren != null; }
		}


		/// <summary>
		/// The starting offset in the parsed text matched by this token.
		/// </summary>
		/// 
		/// <value>
		/// Zero-based offset in characters from start of text.</value>
		
		public int Offset
		{
			get { return mOffset; }
		}


		/// <summary>
		/// The grammar construct which is fully matched by this token, or null
		/// if this token is anonymous.
		/// </summary>
		
		public GrammarConstruct GrammarConstruct
		{
			get { return mGrammarConstruct; }
			internal set { mGrammarConstruct = value; }
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
		/// <remarks>
		/// This operation is invalid if the child token has been replaced by a
		/// value from a token conversion.
		/// </remarks>
		/// 
		/// <param name="index">
		/// Index of the child.</param>
		/// 
		/// <returns>
		/// Child as a Token.</returns>
		/// 
		/// <exception cref="InvalidCastException">
		/// Thrown if child is not a Token.</exception>

		public Token ChildToken(int index)
		{
			return (Token)mChildren[index];
		}


		/// <summary>
		/// Returns true if this token was created by matching the specified
		/// grammar construct name.
		/// </summary>
		/// 
		/// <param name="grammarConstructName">
		/// Name of a parse rule or custom matcher.</param>
		/// 
		/// <returns>
		/// True if the named construct produced this token, false otherwise.
		/// </returns>
		
		public bool Matched(string grammarConstructName)
		{
			if (mGrammarConstruct == null)
				return grammarConstructName == null;

			return mGrammarConstruct.Name == grammarConstructName;
		}


		/// <summary>
		/// Returns the portion of the text which was parsed, as represented by
		/// this token.</summary>
		/// 
		/// <remarks>
		/// This is only valid if this token is the result of a successful
		/// parsing. The results are undefined if parsing failed.
		/// </remarks>
		/// 
		/// <param name="originalText">
		/// Original parsed text (eg. ParseContext.BaseText).</param>
		/// 
		/// <returns>
		/// Portion of original text matched by this token and its children.
		/// </returns>

		public string MatchedText(string originalText)
		{
			return originalText.Substring(mOffset, mEndOffset - mOffset);		   
		}


		/// <summary>
		/// Returns the parse tree beginning at this token as an XML string
		/// representation.
		/// </summary>
		/// 
		/// <remarks>
		/// Parse rule names are expressed as XML element names, and the content
		/// of each element is taken from this token's children and matched
		/// text.
		/// </remarks>
		/// 
		/// <param name="context">
		/// Parsing context which produced this token.</param>
		/// 
		/// <returns>
		/// XML representing the parse subtree of this token.</returns>

		public string ToXml(ParseContext context)
		{
			return ToXml(context.BaseText);
		}


		/// <summary>
		/// Returns the parse tree beginning at this token as an XML string
		/// representation.
		/// </summary>
		/// 
		/// <remarks>
		/// Parse rule names are expressed as XML element names, and the content
		/// of each element is taken from this token's children and matched
		/// text.
		/// </remarks>
		/// 
		/// <param name="parsedText">
		/// Original parsed text (eg. ParseContext.BaseText).</param>
		/// 
		/// <returns>
		/// XML representing the parse subtree of this token.</returns>

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

			if (mGrammarConstruct != null) {
				text.Append('<');
				AppendEscape(text, mGrammarConstruct.Name);
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

			if (hasContent && mGrammarConstruct != null) {
				if (indent)
					AppendIndent(text, indentLevel);
				text.Append("</");
				AppendEscape(text, mGrammarConstruct.Name);
				text.Append('>');
			}
		}
	}
}
