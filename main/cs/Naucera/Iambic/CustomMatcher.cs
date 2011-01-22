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

namespace Naucera.Iambic
{
	/// <summary>
	/// Abstract custom matcher for providing user-defined matching behaviour.
	/// </summary>

	public abstract class CustomMatcher : GrammarConstruct
	{
		/// <summary>
		/// Creates a CustomMatcher with the specified name. The name is used
		/// to identify custom matchers referenced in a parser grammar.
		/// </summary>
		
		protected CustomMatcher(string name) : base(name) {}
		
		
		/// <summary>
		/// <para>
		/// Matches the given text at the specified offset, returning the number
		/// of characters matched from the offset. If no match is found, -1 is
		/// returned instead.</para>
		/// 
		/// <para>
		/// The default implemention throws NotImplementedException.</para>
		/// </summary>
		/// 
		/// <param name="text">
		/// Text to match against.</param>
		/// 
		/// <param name="offset">
		/// Offset at which to match.</param>
		/// 
		/// <returns>
		/// Number of characters matched, or -1.</returns>

		public virtual int Match(string text, int offset)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// <para>
		/// Matches the given text by searching beginning at the specified
		/// offset, returning the number of characters matched from the position
		/// indicated by matchOffset. If no match is found, -1 is returned
		/// instead.
		/// </para>
		/// 
		/// <para>
		/// Subclasses may optionally implement this method to support error
		/// recovery. If error recovery is not required (the default, when the
		/// MaxErrors property of a Parser is set to 1), this method need not
		/// be implemented.</para>
		/// 
		/// <para>
		/// The default implemention throws NotImplementedException.</para>
		/// </summary>
		/// 
		/// <param name="text">
		/// Text to match against.</param>
		/// 
		/// <param name="offset">
		/// Offset at which to begin searching for a match.</param>
		/// 
		/// <param name="matchOffset">
		/// Offset at which a found match begins.</param>
		/// 
		/// <returns>
		/// Number of characters matched, or -1.</returns>

		public virtual int MatchLeniently(string text, int offset, out int matchOffset)
		{
			throw new NotImplementedException();
		}
	}
}
