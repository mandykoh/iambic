namespace Naucera.Iambic
{
	/// <summary>
	/// Exception indicating that a non-terminal grammar construct has been
	/// defined more than once.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class DuplicateConstructException : InvalidGrammarException
	{
		private readonly string constructName;


		/// <summary>
		/// Creates a DuplicateConstructException for the named construct.
		/// </summary>
		
		public DuplicateConstructException(string constructName) : base(constructName)
		{
			this.constructName = constructName;
		}


		public string ConstructName {
			get { return constructName; }
		}
	}
}
