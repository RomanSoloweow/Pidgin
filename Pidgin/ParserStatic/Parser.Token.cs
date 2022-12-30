using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser that parses and returns a single token.
        /// </summary>
        /// <param name="token">The token to parse.</param>
        /// <returns>A parser that parses and returns a single token.</returns>
        public static Parser<TContext, TToken, TToken> Token<TContext, TToken>(TToken token)
            where TContext : IParsingContext
            => new TokenParser<TContext, TToken>(token);

        /// <summary>
        /// Creates a parser that parses and returns a single token satisfying a predicate.
        /// </summary>
        /// <param name="predicate">A predicate function to apply to a token.</param>
        /// <returns>A parser that parses and returns a single token satisfying a predicate.</returns>
        public static Parser<TContext, TToken, TToken> Token<TContext, TToken>(Func<TToken, bool> predicate)
            where TContext : IParsingContext
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return new PredicateTokenParser<TContext, TToken>(predicate);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class TokenParser<TContext, TToken> : Parser<TContext, TToken, TToken>
        where TContext : IParsingContext
    {
        private readonly TToken _token;
        private Expected<TToken> _expected;

        private Expected<TToken> Expected
        {
            get
            {
                if (_expected.Tokens.IsDefault)
                {
                    _expected = new Expected<TToken>(ImmutableArray.Create(_token));
                }

                return _expected;
            }
        }

        public TokenParser(TToken token)
        {
            _token = token;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out TToken result)
        {
            if (!state.HasCurrent)
            {
                state.SetError(Maybe.Nothing<TToken>(), true, state.Location, null);
                expecteds.Add(Expected);
                result = default;
                return false;
            }

            var token = state.Current;
            if (!EqualityComparer<TToken>.Default.Equals(token, _token))
            {
                state.SetError(Maybe.Just(token), false, state.Location, null);
                expecteds.Add(Expected);
                result = default;
                return false;
            }

            state.Advance();
            result = token;
            return true;
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class PredicateTokenParser<TContext, TToken> : Parser<TContext, TToken, TToken>
        where TContext : IParsingContext
    {
        private readonly Func<TToken, bool> _predicate;

        public PredicateTokenParser(Func<TToken, bool> predicate)
        {
            _predicate = predicate;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out TToken result)
        {
            if (!state.HasCurrent)
            {
                state.SetError(Maybe.Nothing<TToken>(), true, state.Location, null);
                result = default;
                return false;
            }

            var token = state.Current;
            if (!_predicate(token))
            {
                state.SetError(Maybe.Just(token), false, state.Location, null);
                result = default;
                return false;
            }

            state.Advance();
            result = token;
            return true;
        }
    }
}
