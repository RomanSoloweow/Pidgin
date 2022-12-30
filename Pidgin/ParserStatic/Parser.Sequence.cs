using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Pidgin.ParsingContext;

using LExpression = System.Linq.Expressions.Expression;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser that parses and returns a literal sequence of tokens.
        /// </summary>
        /// <param name="tokens">A sequence of tokens.</param>
        /// <returns>A parser that parses a literal sequence of tokens.</returns>
        public static Parser<TContext, TToken, TToken[]> Sequence<TContext, TToken>(params TToken[] tokens)
            where TContext : IParsingContext
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            return Sequence<TContext, TToken, TToken[]>(tokens);
        }

        /// <summary>
        /// Creates a parser that parses and returns a literal sequence of tokens.
        /// The input enumerable is enumerated and copied to a list.
        /// </summary>
        /// <param name="tokens">A sequence of tokens.</param>
        /// <returns>A parser that parses a literal sequence of tokens.</returns>
        public static Parser<TContext, TToken, TEnumerable> Sequence<TContext, TToken, TEnumerable>(TEnumerable tokens)
            where TContext : IParsingContext
            where TEnumerable : IEnumerable<TToken>
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            return SequenceTokenParser<TContext, TToken, TEnumerable>.Create(tokens);
        }

        /// <summary>
        /// Creates a parser that applies a sequence of parsers and collects the results.
        /// This parser fails if any of its constituent parsers fail.
        /// </summary>
        /// <typeparam name="T">The return type of the parsers.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TToken"></typeparam>
        /// <param name="parsers">A sequence of parsers.</param>
        /// <returns>A parser that applies a sequence of parsers and collects the results.</returns>
        public static Parser<TContext, TToken, IEnumerable<T>> Sequence<TContext, TToken, T>(params Parser<TContext, TToken, T>[] parsers)
            where TContext : IParsingContext
        {
            return Sequence<TContext, TToken, T>(parsers.AsEnumerable());
        }

        /// <summary>
        /// Creates a parser that applies a sequence of parsers and collects the results.
        /// This parser fails if any of its constituent parsers fail.
        /// </summary>
        /// <typeparam name="T">The return type of the parsers.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TToken"></typeparam>
        /// <param name="parsers">A sequence of parsers.</param>
        /// <returns>A parser that applies a sequence of parsers and collects the results.</returns>
        public static Parser<TContext, TToken, IEnumerable<T>> Sequence<TContext, TToken, T>(IEnumerable<Parser<TContext, TToken, T>> parsers)
            where TContext : IParsingContext
        {
            if (parsers == null)
            {
                throw new ArgumentNullException(nameof(parsers));
            }

            var parsersArray = parsers.ToArray();
            if (parsersArray.Length == 1)
            {
                return parsersArray[0].Select(x => new[] { x }.AsEnumerable());
            }

            return new SequenceParser<TContext, TToken, T>(parsersArray);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class SequenceParser<TContext, TToken, T> : Parser<TContext, TToken, IEnumerable<T>>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, T>[] _parsers;

        public SequenceParser(Parser<TContext, TToken, T>[] parsers)
        {
            _parsers = parsers;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out IEnumerable<T> result)
        {
            var ts = new T[_parsers.Length];

            for (var i = 0; i < _parsers.Length; i++)
            {
                var p = _parsers[i];

                var success = p.TryParse(ref context, ref state, ref expecteds, out ts[i]!);

                if (!success)
                {
                    result = null;
                    return false;
                }
            }

            result = ts;
            return true;
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal static class SequenceTokenParser<TContext, TToken, TEnumerable>
        where TContext : IParsingContext
        where TEnumerable : IEnumerable<TToken>
    {
        private static readonly Func<TEnumerable, Parser<TContext, TToken, TEnumerable>>? _createParser = GetCreateParser();

        public static Parser<TContext, TToken, TEnumerable> Create(TEnumerable tokens)
        {
            if (_createParser != null)
            {
                return _createParser(tokens);
            }

            return new SequenceTokenParserSlow<TContext, TToken, TEnumerable>(tokens);
        }

        private static Func<TEnumerable, Parser<TContext, TToken, TEnumerable>>? GetCreateParser()
        {
            var ttoken = typeof(TToken).GetTypeInfo();
            var equatable = typeof(IEquatable<TToken>).GetTypeInfo();

            if (!ttoken.IsValueType || !equatable.IsAssignableFrom(ttoken))
            {
                return null;
            }

            var ctor = typeof(SequenceTokenParserFast<,,>)
                .MakeGenericType(typeof(TToken), typeof(TEnumerable))
                .GetTypeInfo()
                .DeclaredConstructors
                .Single();
            var param = LExpression.Parameter(typeof(TEnumerable));
            var create = LExpression.New(ctor, param);
            return LExpression.Lambda<Func<TEnumerable, Parser<TContext, TToken, TEnumerable>>>(create, param).Compile();
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class SequenceTokenParserFast<TContext, TToken, TEnumerable> : Parser<TContext, TToken, TEnumerable>
        where TContext : IParsingContext
        where TToken : struct, IEquatable<TToken>
        where TEnumerable : IEnumerable<TToken>
    {
        private readonly TEnumerable _value;
        private readonly ImmutableArray<TToken> _valueTokens;

        public SequenceTokenParserFast(TEnumerable value)
        {
            _value = value;
            _valueTokens = value.ToImmutableArray();
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out TEnumerable result)
        {
            var span = state.LookAhead(_valueTokens.Length);  // span.Length <= _valueTokens.Length

            var errorPos = -1;
            for (var i = 0; i < span.Length; i++)
            {
                if (!span[i].Equals(_valueTokens[i]))
                {
                    errorPos = i;
                    break;
                }
            }

            if (errorPos != -1)
            {
                // strings didn't match
                state.Advance(errorPos);
                state.SetError(
                    Maybe.Just(span[errorPos]),
                    false,
                    state.Location,
                    null
                );
                expecteds.Add(new Expected<TToken>(_valueTokens));
                result = default;
                return false;
            }

            if (span.Length < _valueTokens.Length)
            {
                // strings matched but reached EOF
                state.Advance(span.Length);
                state.SetError(
                    Maybe.Nothing<TToken>(),
                    true,
                    state.Location,
                    null
                );
                expecteds.Add(new Expected<TToken>(_valueTokens));
                result = default;
                return false;
            }

            // OK
            state.Advance(_valueTokens.Length);
            result = _value;
            return true;
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class SequenceTokenParserSlow<TContext, TToken, TEnumerable> : Parser<TContext, TToken, TEnumerable>
        where TContext : IParsingContext
        where TEnumerable : IEnumerable<TToken>
    {
        private readonly TEnumerable _value;
        private readonly ImmutableArray<TToken> _valueTokens;

        public SequenceTokenParserSlow(TEnumerable value)
        {
            _value = value;
            _valueTokens = value.ToImmutableArray();
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out TEnumerable result)
        {
            var span = state.LookAhead(_valueTokens.Length);  // span.Length <= _valueTokens.Length

            var errorPos = -1;
            for (var i = 0; i < span.Length; i++)
            {
                if (!EqualityComparer<TToken>.Default.Equals(span[i], _valueTokens[i]))
                {
                    errorPos = i;
                    break;
                }
            }

            if (errorPos != -1)
            {
                // strings didn't match
                state.Advance(errorPos);
                state.SetError(Maybe.Just(span[errorPos]), false, state.Location, null);
                expecteds.Add(new Expected<TToken>(_valueTokens));
                result = default;
                return false;
            }

            if (span.Length < _valueTokens.Length)
            {
                // strings matched but reached EOF
                state.Advance(span.Length);
                state.SetError(Maybe.Nothing<TToken>(), true, state.Location, null);
                expecteds.Add(new Expected<TToken>(_valueTokens));
                result = default;
                return false;
            }

            // OK
            state.Advance(_valueTokens.Length);
            result = _value;
            return true;
        }
    }
}
