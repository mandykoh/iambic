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
	/// Returns a value to annotate the given token with.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// This is invoked by Parser.Parse(), and only when parsing has succeeded,
	/// and then only after the annotation for all child tokens has been done.
	/// </para>
	///
	/// <para>
	/// When this is invoked, each child of the token is guaranteed to have been
	/// annotated with a value from its own annotation (such that the child's
	/// Value property will return the value), unless the child has no
	/// annotation defined (using the Parser.Annotating() method).</para>
	/// </remarks>
	///
	/// <param name="token">
	/// Token to be annotated.</param>
	/// 
	/// <param name="context">
	/// Parse context from which the token was generated.</param>
	/// 
	/// <param name="parseArgs">
	/// Arguments given via the Parse() method of the parser.</param>
	/// 
	/// <returns>
	/// Value to annotate the token with in the parse tree.</returns>

	public delegate object TokenAnnotation(Token token, ParseContext context, params object[] parseArgs);


	/// <summary>
	/// Returns a value to annotate the given token with.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// This is invoked by Parser.Parse(), and only when parsing has succeeded,
	/// and then only after the annotation for all child tokens has been done.
	/// </para>
	///
	/// <para>
	/// When this is invoked, each child of the token is guaranteed to have been
	/// annotated with a value from its own annotation (such that the child's
	/// Value property will return the value), unless the child has no
	/// annotation defined (using the Parser.Annotating() method).</para>
	/// </remarks>
	///
	/// <param name="token">
	/// Token to be annotated.</param>
	/// 
	/// <param name="context">
	/// Parse context from which the token was generated.</param>
	/// 
	/// <returns>
	/// Value to annotate the token with in the parse tree.</returns>
	
	public delegate object TokenAnnotationWithNoArgs(Token token, ParseContext context);


	/// <summary>
	/// Returns a value to annotate the given token with.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// This is invoked by Parser.Parse(), and only when parsing has succeeded,
	/// and then only after the annotation for all child tokens has been done.
	/// </para>
	///
	/// <para>
	/// When this is invoked, each child of the token is guaranteed to have been
	/// annotated with a value from its own annotation (such that the child's
	/// Value property will return the value), unless the child has no
	/// annotation defined (using the Parser.Annotating() method).</para>
	/// </remarks>
	///
	/// <param name="token">
	/// Token to be annotated.</param>
	/// 
	/// <returns>
	/// Value to annotate the token with in the parse tree.</returns>

	public delegate object TokenAnnotationWithNoContext(Token token);


	/// <summary>
	/// Returns a value to tag a token with.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// This is invoked by Parser.Parse(), and only when parsing has succeeded.
	/// </para>
	///
	/// <para>
	/// When this is invoked, each child of the token being tagged is guaranteed
	/// to have been tagged with a value of its own, unless the child has no tag
	/// defined (using the Parser.Tagging() method).</para>
	/// </remarks>
	///
	/// <returns>
	/// Value to tag a token with in the parse tree.</returns>

	public delegate object TokenAnnotationWithNoToken();
}
