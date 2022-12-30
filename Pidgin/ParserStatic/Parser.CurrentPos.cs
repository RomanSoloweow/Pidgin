using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// A parser which returns the current source position.
        /// </summary>
        public static Parser<TContext, TToken, SourcePosDelta> CurrentSourcePosDelta<TContext, TToken>()
            where TContext : IParsingContext
            => new CurrentPosParser<TContext, TToken>();

        /// <summary>
        /// A parser which returns the current source position.
        /// </summary>
        /// <returns></returns>
        public static Parser<TContext, TToken, SourcePos> CurrentPos<TContext, TToken>()
            where TContext : IParsingContext
            => CurrentSourcePosDelta<TContext, TToken>().Select(d => new SourcePos(1, 1) + d);
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class CurrentPosParser<TContext, TToken> : Parser<TContext, TToken, SourcePosDelta>
        where TContext : IParsingContext
    {
        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out SourcePosDelta result)
        {
            result = state.ComputeSourcePosDelta();
            return true;
        }
    }
}
