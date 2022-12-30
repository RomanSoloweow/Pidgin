using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser<TContext, TToken>
        where TContext : IParsingContext
    {
        /// <summary>
        /// Creates a parser which parses the end of the input stream.
        /// </summary>
        /// <returns>A parser which parses the end of the input stream and returns <see cref="Unit.Value"/>.</returns>
        public static Parser<TContext, TToken, Unit> End { get; } = new EndParser<TContext, TToken>();
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class EndParser<TContext, TToken> : Parser<TContext, TToken, Unit>
        where TContext : IParsingContext
    {
        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out Unit result)
        {
            if (state.HasCurrent)
            {
                state.SetError(Maybe.Just(state.Current), false, state.Location, null);
                expecteds.Add(default);
                result = default;
                return false;
            }

            result = Unit.Value;
            return true;
        }
    }
}
