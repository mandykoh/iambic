using System;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Generic composite expression which contains nested subexpressions.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public abstract class CompositeExpression : ParseExpression
	{
		private ParseExpression[] uncompiled;
		private ParseExpression[] expressions;


		/// <summary>
		/// Creates a CompositeExpression with the specified nested
		/// subexpressions.
		/// </summary>
		///
		/// <param name="expressions">
		/// The expressions to be nested.</param>
		
		protected CompositeExpression(params ParseExpression[] expressions)
		{
			uncompiled = new ParseExpression[expressions.Length];
			Array.Copy(expressions, uncompiled, expressions.Length);
		}


		/// <summary>
		/// The number of child expressions.
		/// </summary>
		
		protected int ExpressionCount {
			get { return expressions.Length; }
		}


		/// <summary>
		/// Compiles each child expression and replaces it with the result.
		/// </sumary>
		///
		/// <exception cref="InvalidOperationException">
		/// Thrown if the expression has already been compiled.</exception>
		/// 
		/// <exception cref="EmptyCompositeException">
		/// Thrown if there are no child expressions.</exception>
		
		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			if (uncompiled == null)
				throw new InvalidOperationException();
			if (uncompiled.Length == 0)
				throw new EmptyCompositeException(this);

			expressions = new ParseExpression[uncompiled.Length];

			for (var i = 0; i < expressions.Length; ++i)
				expressions[i] = uncompiled[i].Compile(parser);

			uncompiled = null;

			return this;
		}


		/// <summary>
		/// Returns the nested child expression at a specified index.
		/// </summary>
		
		protected ParseExpression Expression(int i)
		{
			return expressions[i];
		}
	}
}
