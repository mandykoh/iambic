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

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Generic composite expression which contains nested subexpressions.
	/// </summary>
	
	public abstract class CompositeExpression : ParseExpression
	{
		ParseExpression[] mUncompiled;
		ParseExpression[] mExpressions;


		/// <summary>
		/// Creates a CompositeExpression with the specified nested
		/// subexpressions.
		/// </summary>
		///
		/// <param name="expressions">
		/// The expressions to be nested.</param>
		
		protected CompositeExpression(params ParseExpression[] expressions)
		{
			mUncompiled = new ParseExpression[expressions.Length];
			Array.Copy(expressions, mUncompiled, expressions.Length);
		}


		/// <summary>
		/// The number of child expressions.
		/// </summary>
		
		protected int ExpressionCount
		{
			get { return mExpressions.Length; }
		}


		/// <summary>
		/// Compiles each child expression and replaces it with the result.
		/// </summary>
		///
		/// <exception cref="InvalidOperationException">
		/// Thrown if the expression has already been compiled.</exception>
		/// 
		/// <exception cref="EmptyCompositeException">
		/// Thrown if there are no child expressions.</exception>
		
		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			if (mUncompiled == null)
				throw new InvalidOperationException();
			if (mUncompiled.Length == 0)
				throw new EmptyCompositeException(this);

			mExpressions = new ParseExpression[mUncompiled.Length];

			for (var i = 0; i < mExpressions.Length; ++i)
				mExpressions[i] = mUncompiled[i].Compile(parser);

			mUncompiled = null;

			return this;
		}


		/// <summary>
		/// Returns the nested child expression at a specified index.
		/// </summary>
		
		protected ParseExpression Expression(int i)
		{
			return mExpressions[i];
		}
	}
}
