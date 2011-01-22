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
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
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
