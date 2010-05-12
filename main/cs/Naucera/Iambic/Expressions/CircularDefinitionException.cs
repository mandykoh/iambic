namespace Naucera.Iambic.Expressions
{
    /// <summary>
    /// Exception indicating that a grammar contains a circular definition.
    /// This is a sign that the grammar contains left-recursion and is not
    /// well-formed.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>
    
    public class CircularDefinitionException : InvalidGrammarException
    {
        private readonly string baseRuleName;
        private readonly string referenceName;


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
        
        public CircularDefinitionException(string baseRuleName,
                                           string referenceName)
            : base(CreateMessage(baseRuleName, referenceName))
        {
            this.baseRuleName = baseRuleName;
            this.referenceName = referenceName;
        }


        public string BaseRuleName {
            get { return baseRuleName; }
        }


        public string ReferenceName {
            get { return referenceName; }
        }


        /// <summary>
        /// Returns the exception message for the given arguments.
        /// </summary>

        private static string CreateMessage(string baseRuleName,
                                            string referenceName)
        {
            return "Rule " + baseRuleName + " circularly references " + referenceName;
        }
    }
}