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

using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Abstract interface for a parsing expression.
	/// </summary>

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
		/// <returns>
		/// True if this expression is an optional match.</returns>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown if the expression is not well-formed.</exception>

		internal abstract bool CheckWellFormed(string baseRuleName, HashSet<string> ruleNames);


		/// <summary>
		/// Performs post-initialisation on the expression, after the parsing
		/// rules have been created for the parser.
		/// </summary>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown on grammar error.</exception>

		internal abstract ParseExpression Compile<T>(Parser<T> parser);


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
