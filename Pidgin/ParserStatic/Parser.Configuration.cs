using System.Diagnostics.CodeAnalysis;

using Pidgin.Configuration;
using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>A parser which returns the current <see cref="IConfiguration{TToken}"/>.</summary>
        public static Parser<TContext, TToken, IConfiguration<TToken>> Configuration<TContext, TToken>()
            where TContext : IParsingContext
            => new ConfigurationParser<TContext, TToken>();
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal class ConfigurationParser<TContext, TToken> : Parser<TContext, TToken, IConfiguration<TToken>>
        where TContext : IParsingContext
    {
        public override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out IConfiguration<TToken> result)
        {
            result = state.Configuration;
            return true;
        }
    }
}
