using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// A parser that parses and returns either the literal string "\r\n" or the literal string "\n".
        /// </summary>
        public static Parser<TContext, char, string> EndOfLine<TContext>()
            where TContext : IParsingContext
            => String<TContext>("\r\n")
                .Or(String<TContext>("\n"))
                .Labelled("end of line");
    }
}
