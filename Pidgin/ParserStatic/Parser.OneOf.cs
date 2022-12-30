using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which parses and returns one of the specified characters.
        /// </summary>
        /// <param name="chars">A sequence of characters to choose between.</param>
        /// <returns>A parser which parses and returns one of the specified characters.</returns>
        public static Parser<TContext, char, char> OneOf<TContext>(params char[] chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            return OneOf<TContext>(chars.AsEnumerable());
        }

        /// <summary>
        /// Creates a parser which parses and returns one of the specified characters.
        /// </summary>
        /// <param name="chars">A sequence of characters to choose between.</param>
        /// <returns>A parser which parses and returns one of the specified characters.</returns>
        public static Parser<TContext, char, char> OneOf<TContext>(IEnumerable<char> chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            var cs = chars.ToArray();
            return Parser<TContext, char>
                .Token(c => Array.IndexOf(cs, c) != -1)
                .WithExpected(cs.Select(c => new Expected<char>(ImmutableArray.Create(c))).ToImmutableArray());
        }

        /// <summary>
        /// Creates a parser which parses and returns one of the specified characters, in a case insensitive manner.
        /// The parser returns the actual character parsed.
        /// </summary>
        /// <param name="chars">A sequence of characters to choose between.</param>
        /// <returns>A parser which parses and returns one of the specified characters, in a case insensitive manner.</returns>
        public static Parser<TContext, char, char> CIOneOf<TContext>(params char[] chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            return CIOneOf<TContext>(chars.AsEnumerable());
        }

        /// <summary>
        /// Creates a parser which parses and returns one of the specified characters, in a case insensitive manner.
        /// The parser returns the actual character parsed.
        /// </summary>
        /// <param name="chars">A sequence of characters to choose between.</param>
        /// <returns>A parser which parses and returns one of the specified characters, in a case insensitive manner.</returns>
        public static Parser<TContext, char, char> CIOneOf<TContext>(IEnumerable<char> chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            var cs = chars.Select(char.ToLowerInvariant).ToArray();
            var builder = ImmutableArray.CreateBuilder<Expected<char>>(cs.Length * 2);
            foreach (var c in cs)
            {
                builder.Add(new Expected<char>(ImmutableArray.Create(char.ToLowerInvariant(c))));
                builder.Add(new Expected<char>(ImmutableArray.Create(char.ToUpperInvariant(c))));
            }

            return Parser
                .Token(c => Array.IndexOf(cs, char.ToLowerInvariant(c)) != -1)
                .WithExpected(builder.MoveToImmutable());
        }

        /// <summary>
        /// Creates a parser which applies one of the specified parsers.
        /// The resulting parser fails if all of the input parsers fail without consuming input, or if one of them fails after consuming input.
        /// </summary>
        /// <typeparam name="TToken">The type of tokens in the parsers' input stream.</typeparam>
        /// <typeparam name="T">The return type of the parsers.</typeparam>
        /// <param name="parsers">A sequence of parsers to choose between.</param>
        /// <returns>A parser which applies one of the specified parsers.</returns>
        public static Parser<TContext, TToken, T> OneOf<TContext, TToken, T>(params Parser<TContext, TToken, T>[] parsers)
            where TContext : IParsingContext
        {
            if (parsers == null)
            {
                throw new ArgumentNullException(nameof(parsers));
            }

            return OneOf(parsers.AsEnumerable());
        }

        /// <summary>
        /// Creates a parser which applies one of the specified parsers.
        /// The resulting parser fails if all of the input parsers fail without consuming input, or if one of them fails after consuming input.
        /// The input enumerable is enumerated and copied to a list.
        /// </summary>
        /// <typeparam name="TToken">The type of tokens in the parsers' input stream.</typeparam>
        /// <typeparam name="T">The return type of the parsers.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="parsers">A sequence of parsers to choose between.</param>
        /// <returns>A parser which applies one of the specified parsers.</returns>
        public static Parser<TContext, TToken, T> OneOf<TContext, TToken, T>(IEnumerable<Parser<TContext, TToken, T>> parsers)
            where TContext : IParsingContext
        {
            if (parsers == null)
            {
                throw new ArgumentNullException(nameof(parsers));
            }

            return OneOfParser<TContext, TToken, T>.Create(parsers);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class OneOfParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, T>[] _parsers;

        private OneOfParser(Parser<TContext, TToken, T>[] parsers)
        {
            _parsers = parsers;
        }

        // see comment about expecteds in ParseState.Error.cs
        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result)
        {
            var firstTime = true;
            var err = new InternalError<TToken>(
                Maybe.Nothing<TToken>(),
                false,
                state.Location,
                "OneOf had no arguments"
            );

            var childExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());  // the expecteds for all loop iterations
            var grandchildExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());  // the expecteds for the current loop iteration
            foreach (var p in _parsers)
            {
                var thisStartLoc = state.Location;

                if (p.TryParse(ref context, ref state, ref grandchildExpecteds, out result))
                {
                    // throw out all expecteds
                    grandchildExpecteds.Dispose();
                    childExpecteds.Dispose();
                    return true;
                }

                // we'll usually return the error from the first parser that didn't backtrack,
                // even if other parsers had a longer match.
                // There is some room for improvement here.
                if (state.Location > thisStartLoc)
                {
                    // throw out all expecteds except this one
                    expecteds.AddRange(grandchildExpecteds.AsSpan());
                    childExpecteds.Dispose();
                    grandchildExpecteds.Dispose();
                    result = default;
                    return false;
                }

                childExpecteds.AddRange(grandchildExpecteds.AsSpan());
                grandchildExpecteds.Clear();

                // choose the longest match, preferring the left-most error in a tie,
                // except the first time (avoid returning "OneOf had no arguments").
                if (firstTime || state.ErrorLocation > err.ErrorLocation)
                {
                    err = state.GetError();
                }

                firstTime = false;
            }

            state.SetError(err);
            expecteds.AddRange(childExpecteds.AsSpan());
            childExpecteds.Dispose();
            grandchildExpecteds.Dispose();
            result = default;
            return false;
        }

        internal static OneOfParser<TContext, TToken, T> Create(IEnumerable<Parser<TContext, TToken, T>> parsers)
        {
            // if we know the length of the collection,
            // we know we're going to need at least that much room in the list
            var list = parsers is ICollection<Parser<TContext, TToken, T>> coll
                ? new List<Parser<TContext, TToken, T>>(coll.Count)
                : new List<Parser<TContext, TToken, T>>();

            foreach (var p in parsers)
            {
                if (p == null)
                {
                    throw new ArgumentNullException(nameof(parsers));
                }

                if (p is OneOfParser<TContext, TToken, T> o)
                {
                    list.AddRange(o._parsers);
                }
                else
                {
                    list.Add(p);
                }
            }

            return new OneOfParser<TContext, TToken, T>(list.ToArray());
        }
    }
}
