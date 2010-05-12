namespace Naucera.Iambic
{
    /// <summary>
    /// <para>
    /// Processes the given token, with its associated parsing context. This is
    /// invoked only when parsing has succeeded, and then only after the
    /// processors for all child tokens have been invoked.</para>
    ///
    /// <para>
    /// When this is invoked, each child of the token will have been replaced
    /// with the return value from its own processor. If a child has no
    /// processor, it is left unchanged.</para>
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

    public delegate object TokenProcessorDelegate(Token token, ParseContext context, params object[] parseArgs);
}