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
		private const int MismatchLength = 32;

		private readonly ParseContext context;
		private readonly Token result;


		/// <summary>
		/// Creates a SyntaxException with the specified parsing context and
		/// incomplete parse result.
		/// </summary>
		
		public SyntaxException(ParseContext context, Token result)
			: base(BuildErrorMessages(context))
		{
			this.context = context;
			this.result = result;
		}


		/// <summary>
		/// The parsing context.
		/// </summary>
		
		public ParseContext Context {
			get { return context; }
		}


		/// <summary>
		/// The result of the parsing.
		/// </summary>
		
		public Token Result {
			get { return result; }
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
