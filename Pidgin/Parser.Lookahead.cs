using System;
using System.Collections.Immutable;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// If <paramref name="parser"/> succeeds, <c>Lookahead(parser)</c> backtracks,
        /// behaving as if <paramref name="parser"/> had not consumed any input.
        /// No backtracking is performed upon failure.
        /// </summary>
        /// <param name="parser">The parser to look ahead with</param>
        /// <returns>A parser which rewinds the input stream if <paramref name="parser"/> succeeds.</returns>
        public static Parser<TToken, T> Lookahead<TToken, T>(Parser<TToken, T> parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }
            return new LookaheadParser<TToken, T>(parser);
        }

        private class LookaheadParser<TToken, T> : Parser<TToken, T>
        {
            private readonly Parser<TToken, T> _parser;

            public LookaheadParser(Parser<TToken, T> parser)
            {
                _parser = parser;
            }

            private protected override ImmutableSortedSet<Expected<TToken>> CalculateExpected()
                => _parser.Expected;

            internal override InternalResult<T> Parse(ref ParseState<TToken> state)
            {
                state.PushBookmark();

                var result = _parser.Parse(ref state);

                if (result.Success)
                {
                    state.Rewind();
                    return InternalResult.Success<T>(result.Value, false);
                }
                state.PopBookmark();
                return result;
            }
        }
    }
}