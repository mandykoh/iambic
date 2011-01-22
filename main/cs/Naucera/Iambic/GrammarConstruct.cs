﻿#region license

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

namespace Naucera.Iambic
{
	/// <summary>
	/// Abstract implementation of a parser grammar construct.
	/// </summary>

	public abstract class GrammarConstruct
	{
		readonly string mName;
		TokenConversion mTokenConversion;


		protected GrammarConstruct(string name)
		{
			mName = name;
		}


		/// <summary>
		/// Flag indicating whether a token conversion has been set for this construct.
		/// </summary>

		public bool HasConversion
		{
			get { return mTokenConversion != null; }
		}


		public string Name
		{
			get { return mName; }
		}


		internal object ReplaceToken(Token token, ParseContext context, params object[] args)
		{
			if (mTokenConversion == null)
				return null;

			return mTokenConversion(token, context, args);
		}


		/// <summary>
		/// Sets the conversion to use for replacing tokens generated by this
		/// grammar construct with values, or disabling conversion if null is
		/// specified.
		/// </summary>
		/// 
		/// <param name="conversion">
		/// Conversion for tokens parsed by this grammar construct, or null.
		/// </param>
		/// 
		/// <returns>
		/// This grammar construct.</returns>

		public GrammarConstruct ReplacingMatchesWith(TokenConversion conversion)
		{
			mTokenConversion = conversion;
			return this;
		}
	}
}
