using System;

namespace Naucera.Iambic
{
	/// <summary>
	/// General exception for invalid parsing grammar definitions.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>

	public abstract class InvalidGrammarException : Exception
	{
		/// <summary>
		/// Creates an InvalidGrammarException with the specfied message.
		/// </summary>

		protected InvalidGrammarException(string message) : base(message) {}
	}
}
