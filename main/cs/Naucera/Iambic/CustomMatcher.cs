using System;

namespace Naucera.Iambic
{
    /// <summary>
    /// Abstract custom matcher for providing user-defined matching behaviour.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>Copyright (C) 2010 by Amanda Koh.</para>
    /// </remarks>

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