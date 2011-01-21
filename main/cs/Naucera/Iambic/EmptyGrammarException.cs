namespace Naucera.Iambic
{
	/// <summary>
	/// Exception indicating that no rules were specified for a grammar.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class EmptyGrammarException : InvalidGrammarException
	{
		/// <summary>
		/// Creates an EmptyGrammarException.
		/// </summary>
		
		public EmptyGrammarException() : base(CreateMessage()) {}


		/// <summary>
		/// Returns the default exception message.
		/// </summary>

		private static string CreateMessage()
		{
			return "Grammar requires at least one rule to be specified";
		}
	}
}
