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

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Exception indicating that a grammar contains a circular definition.
	/// This is a sign that the grammar contains left-recursion and is not
	/// well-formed.
	/// </summary>
	
	public class CircularDefinitionException : InvalidGrammarException
	{
		readonly string mBaseRuleName;
		readonly string mReferenceName;


		/// <summary>
		/// Creates a CircularDefinitionException to indicate a circular
		/// definition of the specified reference, reached from the given rule.
		/// </summary>
		///
		/// <param name="baseRuleName">
		/// Name of the rule from which the circular reference was reached.
		/// </param>
		/// 
		/// <param name="referenceName">
		/// Name of the rule which is circularly referenced.</param>
		
		public CircularDefinitionException(string baseRuleName, string referenceName)
			: base(CreateMessage(baseRuleName, referenceName))
		{
			mBaseRuleName = baseRuleName;
			mReferenceName = referenceName;
		}


		public string BaseRuleName
		{
			get { return mBaseRuleName; }
		}


		public string ReferenceName
		{
			get { return mReferenceName; }
		}


		/// <summary>
		/// Returns the exception message for the given arguments.
		/// </summary>

		static string CreateMessage(string baseRuleName, string referenceName)
		{
			return "Rule " + baseRuleName + " circularly references " + referenceName;
		}
	}
}
