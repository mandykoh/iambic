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
using System.Text;

namespace Naucera.Iambic
{
	/// <summary>
	/// Exception indicating a syntax error encountered during parsing.
	/// </summary>
	
	public class SyntaxException : Exception
	{
		private const int MismatchLength = 32;

		private readonly ParseContext mContext;
		private readonly Token mResult;


		/// <summary>
		/// Creates a SyntaxException with the specified parsing context and
		/// incomplete parse result.
		/// </summary>
		
		public SyntaxException(ParseContext context, Token result)
			: base(BuildErrorMessages(context))
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
		/// The result of the parsing.
		/// </summary>
		
		public Token Result {
			get { return mResult; }
		}


		private static string BuildErrorMessages(ParseContext context)
		{
			var newLine = Environment.NewLine;
			var text = new StringBuilder();
			var textLength = context.BaseText.Length;

			if (context.ErrorCount > 1)
				text.Append(newLine);

			for (var i = 0; i < context.ErrorCount; ++i) {
				var token = context.GetError(i);

				if (i > 0)
					text.Append(newLine);

				if (context.ErrorCount > 1)
					text.Append(' ');

				var found = context.BaseText.Substring(token.Offset);

				// Truncate the "found" text before the start of the next error
				if (i + 1 < context.ErrorCount) {
					var nextError = context.GetError(i + 1);
					var length = nextError.Offset - token.Offset;
					if (found.Length > length)
						found = found.Substring(0, length);
				}

				if (found.Length > 0) {
					text.Append("Expected " + context.Expected + " but found "
						+ Truncate(found) + " when matching " + GetConstructName(token.Origin));
				}
				else if (token.Offset + found.Length >= textLength) {
					text.Append("Expected " + context.Expected
						+ " but reached end of input when matching " + GetConstructName(token.Origin));
				}
				else {
					text.Append("Expected " + context.Expected
						+ " but was missing when matching " + GetConstructName(token.Origin));
				}
			}

			return text.ToString();
		}


		private static string GetConstructName(GrammarConstruct construct)
		{
			return construct == null ? "null" : construct.Name;
		}


		private static string Truncate(string text)
		{
			if (text == null)
				return null;

			if (text.Length > MismatchLength)
				return text.Substring(0, MismatchLength) + "...";

			return text;
		}
	}
}
