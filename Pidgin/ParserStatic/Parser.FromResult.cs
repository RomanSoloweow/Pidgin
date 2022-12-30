using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which returns the specified value without consuming any input.
        /// </summary>
        /// <param name="result">The value to return.</param>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TToken"></typeparam>
        /// <returns>A parser which returns the specified value without consuming any input.</returns>
        public static Parser<TContext, TToken, T> FromResult<TContext, TToken, T>(T result)
            where TContext : IParsingContext
            => Return<TContext, TToken, T>(result);
    }
}
