using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which returns the specified value without consuming any input.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TToken"></typeparam>
        /// <returns>A parser which returns the specified value without consuming any input.</returns>
        public static Parser<TContext, TToken, T> Return<TContext, TToken, T>(T value)
            where TContext : IParsingContext
            => new ReturnParser<TContext, TToken, T>(value);
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class ReturnParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private readonly T _value;

        public ReturnParser(T value)
        {
            _value = value;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out T result)
        {
            result = _value;
            return true;
        }
    }
}
