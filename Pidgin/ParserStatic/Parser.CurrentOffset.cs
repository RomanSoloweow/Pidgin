using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser<TContext, TToken>
        where TContext : IParsingContext
    {
        /// <summary>
        /// A parser which returns the number of input tokens which have been consumed.
        /// </summary>
        public static Parser<TContext, TToken, int> CurrentOffset { get; }
            = new CurrentOffsetParser<TContext, TToken>();
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class CurrentOffsetParser<TContext, TToken> : Parser<TContext, TToken, int>
        where TContext : IParsingContext
    {
        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out int result)
        {
            result = state.Location;
            return true;
        }
    }
}
