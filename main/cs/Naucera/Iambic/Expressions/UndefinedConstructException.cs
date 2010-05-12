namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Exception indicating that a referenced non-terminal was undefined in a
    /// parse grammar.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class UndefinedConstructException : InvalidGrammarException
    {
        private readonly string constructName;


        /// <summary>
        /// Creates an UndefinedConstructException with the name of the undefined
        /// non-terminal.
        /// </summary>
        ///
        /// <param name="constructName">
        /// Name of the non-terminal which has not been defined.</param>
        
        public UndefinedConstructException(string constructName) : base(constructName)
        {
            this.constructName = constructName;
        }


        public string ConstructName {
            get { return constructName; }
        }
    }
}