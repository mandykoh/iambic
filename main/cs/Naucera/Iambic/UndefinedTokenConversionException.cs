using System;

namespace Naucera.Iambic
{
	/// <summary>
	/// Exception thrown when a required token conversion is not defined.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2011 by Amanda Koh.</para>
	/// </remarks>

	public class UndefinedTokenConversionException : Exception
	{
		/// <summary>
		/// Creates an UndefinedTokenConversionException for a grammar rule.
		/// </summary>

		public UndefinedTokenConversionException(ParseRule rule) : base(rule.Name) {}
	}
}
