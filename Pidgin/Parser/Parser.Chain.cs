using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.Configuration;
using Pidgin.ParsingContext;

namespace Pidgin
{
    public partial class Parser<TContext, TToken, T>
    {
        internal Parser<TContext, TToken, U> ChainAtLeastOnce<U, TChainer>(Func<IConfiguration<TToken>, TChainer> factory)
            where TChainer : struct, IChainer<T, U>
            => new ChainAtLeastOnceLParser<TContext, TToken, T, U, TChainer>(this, factory);
    }

    internal interface IChainer<in T, out U>
    {
        void Apply(T value);

        U GetResult();

        void OnError();
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal class ChainAtLeastOnceLParser<TContext, TToken, T, U, TChainer> : Parser<TContext, TToken, U>
        where TChainer : struct, IChainer<T, U>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, T> _parser;
        private readonly Func<IConfiguration<TToken>, TChainer> _factory;

        public ChainAtLeastOnceLParser(Parser<TContext, TToken, T> parser, Func<IConfiguration<TToken>, TChainer> factory)
        {
            _parser = parser;
            _factory = factory;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out U result)
        {
            if (!_parser.TryParse(ref context, ref state, ref expecteds, out var result1))
            {
                // state.Error set by _parser
                result = default;
                return false;
            }

            var chainer = _factory(state.Configuration);
            chainer.Apply(result1);

            var lastStartLoc = state.Location;
            var childExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());
            while (_parser.TryParse(ref context, ref state, ref childExpecteds, out var childResult))
            {
                var endLoc = state.Location;
                childExpecteds.Clear();
                if (endLoc <= lastStartLoc)
                {
                    childExpecteds.Dispose();
                    chainer.OnError();
                    throw new InvalidOperationException("Many() used with a parser which consumed no input");
                }

                chainer.Apply(childResult);

                lastStartLoc = endLoc;
            }

            var lastParserConsumedInput = state.Location > lastStartLoc;
            if (lastParserConsumedInput)
            {
                expecteds.AddRange(childExpecteds.AsSpan());
            }

            childExpecteds.Dispose();

            if (lastParserConsumedInput)
            {
                // the most recent parser failed after consuming input.
                // state.Error was set by _parser
                chainer.OnError();
                result = default;
                return false;
            }

            result = chainer.GetResult();
            return true;
        }
    }
}
