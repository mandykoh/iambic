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

namespace Naucera.Iambic
{
	/// <summary>
	/// <para>
	/// Converts the given token to a value. This is invoked by Parser.Parse(),
	/// and only when parsing has succeeded, and then only after the conversions
	/// for all child tokens have been invoked.</para>
	///
	/// <para>
	/// When this is invoked, each child of the token will have been replaced
	/// with a value from its own conversion. If a child has no conversion, it
	/// is left unchanged as a Token.</para>
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2011 by Amanda Koh.</para>
	/// </remarks>
	///
	/// <param name="context">
	/// Parse context from which the token was generated.</param>
	/// 
	/// <param name="token">
	/// Token to be processed.</param>
	/// 
	/// <param name="parseArgs">
	/// Arguments given via the Parse method of the parser.</param>
	/// 
	/// <returns>
	/// Value to replace the token with in the parse tree.</returns>

	public delegate object TokenConversion(Token token, ParseContext context, params object[] parseArgs);
}
