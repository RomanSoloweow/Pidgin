using System;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public abstract partial class Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        /// <summary>
        /// Creates a parser that applies the current parser followed by the specified parser.
        /// The resulting parser returns the result of the current parser, ignoring the result of the second parser.
        /// </summary>
        /// <param name="parser">The parser to apply after applying the current parser.</param>
        /// <typeparam name="U">The type of the value returned by the second parser.</typeparam>
        /// <returns>A parser that applies the current parser followed by the specified parser.</returns>
        public Parser<TContext, TToken, T> Before<U>(Parser<TContext, TToken, U> parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            return Then(parser, (t, u) => t);
        }
    }
}
