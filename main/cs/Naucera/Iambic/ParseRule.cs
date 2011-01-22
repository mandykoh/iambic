using System;
using System.Collections.Generic;
using System.Text;
using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
	/// <summary>
	/// <para>
	/// Single named parsing grammar rule.</para>
	/// 
	/// <para>
	/// Each ParseRule can belong to one and only one Parser. It is an error
	/// to add a given ParseRule to more than one Parser.</para>
	/// </summary>
	///
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>

	public sealed class ParseRule : GrammarConstruct
	{
		private const int UncompiledRuleIndex = -1;

		private ParseExpression expression;
		private int index = UncompiledRuleIndex;


		/// <summary>
		/// Creates a ParseRule with the specified name and expression.
		/// </summary>

		public ParseRule(string name,
						 ParseExpression expr) : base(name)
		{
			this.expression = expr;
		}


		public ParseExpression Expression {
			get { return expression; }
		}


		internal int Index {
			get { return index; }
		}


		/// <summary>
		/// Checks for well-formedness by ensuring that no left-recursion loops
		/// exist in the expression, throwing an exception if so.
		/// </summary>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown if the grammar is not well-formed.</exception>
		
		internal void CheckWellFormed()
		{
			var ruleNames = new HashSet<string> { Name };
			expression.CheckWellFormed(Name, ruleNames);
		}


		/// <summary>
		/// Compiles this rule for its parser.
		/// </summary>
		/// 
		/// <param name="parser">
		/// Parser to compile for.</param>
		/// 
		/// <exception cref="InvalidOperationException">
		/// Thrown if the rule has already been compiled.</exception>

		internal void Compile<T>(Parser<T> parser)
		{
			if (index != UncompiledRuleIndex)
				throw new InvalidOperationException();

			index = parser.IndexForRule(this);
			expression = expression.Compile(parser);
		}


		internal bool Parse(ParseContext context, out Token result)
		{
			var useCache = !context.Recovering;

			// Attempt to fetch a previously cached result if we are not
			// recovering from a parse error.
			if (useCache) {
				var cached = context.UseCachedResult(this);
				if (cached != null) {
					result = cached.Result;
					return cached.Accepted;
				}
			}

			var startOffset = context.Offset;
			Token token;
			var accepted = expression.Parse(context, this, out token);

			// Decorate the result if parsing succeeded
			if (!context.HasErrors && accepted) {
				var endOffset = context.Offset;
				context.Offset = startOffset;

				Token res;
				context.Accept(this, out res);
				res.EndOffset = token.EndOffset;
				res.Add(token);
				token = res;

				context.Offset = endOffset;
			}

			// Cache the parsed result
			if (useCache && context.MarkedError == null)
				context.CacheResult(this, startOffset, accepted, token);

			result = token;
			return accepted;
		}


		public override string ToString()
		{
			var text = new StringBuilder();
			ToString(text);

			return text.ToString();
		}


		internal void ToString(StringBuilder text)
		{
			text.Append(Name);
			text.Append(" := ");
			expression.ToString(text);
		}
	}
}
