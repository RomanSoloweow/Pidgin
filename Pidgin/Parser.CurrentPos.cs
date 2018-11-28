using System.Collections.Immutable;

namespace Pidgin
{
    public static partial class Parser<TToken>
    {
        /// <summary>
        /// A parser which returns the current source position
        /// </summary>
        /// <returns>A parser which returns the current source position</returns>
        public static Parser<TToken, SourcePos> CurrentPos { get; }
            = new CurrentPosParser();

        private sealed class CurrentPosParser : Parser<TToken, SourcePos>
        {
            private protected override ImmutableSortedSet<Expected<TToken>> CalculateExpected()
                => ExpectedUtil<TToken>.Nil;

            internal override InternalResult<SourcePos> Parse(ref ParseState<TToken> state)
                => InternalResult.Success(state.SourcePos, false);
        }
    }
}