using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which applies <paramref name="parser"/> and backtracks upon failure.
        /// </summary>
        /// <typeparam name="TToken">The type of tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The return type of the parser.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="parser">The parser.</param>
        /// <returns>A parser which applies <paramref name="parser"/> and backtracks upon failure.</returns>
        public static Parser<TContext, TToken, T> Try<TContext, TToken, T>(Parser<TContext, TToken, T> parser)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            return new TryParser<TContext, TToken, T>(parser);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class TryParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, T> _parser;

        public TryParser(Parser<TContext, TToken, T> parser)
        {
            _parser = parser;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result)
        {
            // start buffering the input
            var bookmark = state.Bookmark();
            if (!_parser.TryParse(ref context, ref state, ref expecteds, out result))
            {
                // return to the start of the buffer and discard the bookmark
                state.Rewind(bookmark);
                return false;
            }

            // discard the buffer
            state.DiscardBookmark(bookmark);
            return true;
        }
    }
}
