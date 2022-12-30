using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which always fails without consuming any input.
        /// </summary>
        /// <param name="message">A custom error message.</param>
        /// <typeparam name="T">The return type of the resulting parser.</typeparam>
        /// <returns>A parser which always fails.</returns>
        public static Parser<TContext, TToken, T> Fail<TContext, TToken, T>(string message = "Failed")
            where TContext : IParsingContext
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new FailParser<TContext, TToken, T>(message);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class FailParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private static readonly Expected<TToken> _expected
            = new(ImmutableArray<TToken>.Empty);

        private readonly string _message;

        public FailParser(string message)
        {
            _message = message;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result)
        {
            state.SetError(Maybe.Nothing<TToken>(), false, state.Location, _message);
            expecteds.Add(_expected);
            result = default;
            return false;
        }
    }
}
