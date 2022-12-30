using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// If <paramref name="parser"/> succeeds, <c>Lookahead(parser)</c> backtracks,
        /// behaving as if <paramref name="parser"/> had not consumed any input.
        /// No backtracking is performed upon failure.
        /// </summary>
        /// <param name="parser">The parser to look ahead with.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <returns>A parser which rewinds the input stream if <paramref name="parser"/> succeeds.</returns>
        public static Parser<TContext, TToken, T> Lookahead<TContext, TToken, T>(Parser<TContext, TToken, T> parser)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            return new LookaheadParser<TContext, TToken, T>(parser);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class LookaheadParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, T> _parser;

        public LookaheadParser(Parser<TContext, TToken, T> parser)
        {
            _parser = parser;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result)
        {
            var bookmark = state.Bookmark();

            if (_parser.TryParse(ref context, ref state, ref expecteds, out result))
            {
                state.Rewind(bookmark);
                return true;
            }

            state.DiscardBookmark(bookmark);
            return false;
        }
    }
}
