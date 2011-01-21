using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Abstract interface for a parsing expression.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>

	public abstract class ParseExpression
	{
		/// <summary>
		/// <para>
		/// Checks whether the expression is well-formed, throwing an exception
		/// if a degenerate loop (caused by left-recursion) is found.</para>
		///
		/// <para>
		/// As rule references are encountered as the first term in parsing
		/// expressions, their respective rules are added to the set of seen
		/// rule names.</para>
		/// </summary>
		/// 
		/// <param name="baseRuleName">
		/// Name of the rule being examined for left-recursion.</param>
		/// 
		/// <param name="ruleNames">
		/// Set of rule names that have already been seen.</param>
		/// 
		/// <returns>True if this expression is an optional match.</returns>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown if the expression is not well-formed.</exception>

		internal abstract bool CheckWellFormed(
			string baseRuleName,
			HashSet<string> ruleNames);


		/// <summary>
		/// Performs post-initialisation on the expression, after the parsing
		/// rules have been created for the parser.
		/// </summary>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown on grammar error.</exception>

		internal abstract ParseExpression Compile(Parser parser);


		/// <summary>
		/// Evaluates the expression based on the given context and parsing
		/// grammar rule, returning true if the text was accepted.
		/// </summary>

		internal abstract bool Parse(ParseContext context, ParseRule rule, out Token result);


		/// <summary>
		/// Returns a string representation of the expression. The string is
		/// returned in the grammar specification language described in the
		/// ParserFactory documentation.
		/// </summary>

		public override string ToString()
		{
			var text = new StringBuilder();
			ToString(text);

			return text.ToString();
		}


		/// <summary>
		/// Appends a textual representation of the expression to the specified
		/// StringBuffer. The appended string is in the grammar specification
		/// language described in the ParserFactory documentation.
		/// </summary>

		public abstract void ToString(StringBuilder text);


		/// <summary>
		/// Abstract interface for an internal expression state memento.
		/// </summary>

		internal abstract class Memento
		{
			/**
			 * <p>Returns a copy of the memento.</p>
			 */
			internal abstract Memento Copy();
		}
	}
}
