using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which accepts a string if it matches a fixed string.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class LiteralTerminal : ParseExpression
	{
		private readonly string pattern;


		/// <summary>
		/// Creates a LiteralTerminal expression which matches the specified
		/// string pattern.
		/// </summary>

		public LiteralTerminal(string pattern)
		{
			this.pattern = pattern;
		}


		internal sealed override bool CheckWellFormed(
			string baseRuleName,
			HashSet<string> ruleNames)
		{
			return false;
		}


		internal sealed override ParseExpression Compile(Parser parser)
		{
			return this;
		}


		/// <summary>
		/// Escapes the specified string and returns the grammar literal that
		/// would match it.
		/// </summary>

		public static string Escape(string text)
		{
			var pattern = new StringBuilder();

			pattern.Append('\'');

			for (var i = 0; i < text.Length; ++i) {
				var c = text[i];

				switch (c) {
					case '\'': pattern.Append("\\'"); break;
					case '\\': pattern.Append("\\\\"); break;
					default: pattern.Append(c); break;
				}
			}

			pattern.Append('\'');

			return pattern.ToString();
		}


		internal sealed override bool Parse(ParseContext context,
											ParseRule rule,
											out Token result)
		{
			if (context.Recovering)
				return context.EndRecovery().Accept(out result);

			// Leniently find the next matching string if we are compensating
			// for an earlier error.
			if (context.Compensating) {
				var index = context.BaseText.IndexOf(pattern, context.Offset);

				if (index != -1) {
					context.Offset = index;
					return context.EndCompensation().Accept(pattern.Length, out result);
				}

				return context.Accept(out result);
			}

			// Otherwise, look for a precise match at the current position
			if (pattern.Length <= context.BaseText.Length - context.Offset && context.BaseText.IndexOf(pattern, context.Offset, pattern.Length) != -1)
				return context.Accept(pattern.Length, out result);

			return context.RejectAndMark(rule, this, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append(Escape(pattern));
		}


		/// <summary>
		/// Unescapes the specified grammar pattern and returns the literal
		/// string that would be matched. The result of calling this on an
		/// invalid grammar string literal is undefined.
		/// </summary>
		
		public static string Unescape(string pattern)
		{
			var text = new StringBuilder();

			for (var i = 1; i < pattern.Length - 1; ++i) {
				var c = pattern[i];

				if (c == '\\' && i < pattern.Length - 1) {
					var next = pattern[i + 1];
					if (next == '\\' || next == '\'') {
						text.Append(next);
						++i;
						continue;
					}
				}

				text.Append(c);
			}

			return text.ToString();
		}
	}
}
