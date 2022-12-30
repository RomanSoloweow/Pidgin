using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser that parses any single character.
        /// </summary>
        /// <returns>A parser that parses any single character.</returns>
        public static Parser<TContext, TToken, TToken> Any<TContext, TToken>()
            where TContext : IParsingContext
            => Token<TContext, TToken>(_ => true).Labelled("any character");
    }
}
