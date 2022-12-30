using System;

using Pidgin.ParsingContext;

namespace Pidgin.Permutation
{
    internal abstract class PermutationParserBranch<TContext, TToken, T>
        where TContext : IParsingContext
    {
        public abstract PermutationParserBranch<TContext, TToken, R> Add<U, R>(Parser<TContext, TToken, U> parser, Func<T, U, R> resultSelector);

        public abstract PermutationParserBranch<TContext, TToken, R> AddOptional<U, R>(Parser<TContext, TToken, U> parser, Func<U> defaultValueFactory, Func<T, U, R> resultSelector);

        public abstract Parser<TContext, TToken, T> Build();
    }

#pragma warning disable SA1402  // "File may only contain a single type"
    internal sealed class PermutationParserBranchImpl<TContext, TToken, U, T, R> : PermutationParserBranch<TContext, TToken, R>
        where TContext : IParsingContext
#pragma warning restore SA1402  // "File may only contain a single type"
    {
        private readonly Parser<TContext, TToken, U> _parser;
        private readonly PermutationParser<TContext, TToken, T> _perm;
        private readonly Func<T, U, R> _func;

        public PermutationParserBranchImpl(Parser<TContext, TToken, U> parser, PermutationParser<TContext, TToken, T> perm, Func<T, U, R> func)
        {
            _parser = parser;
            _perm = perm;
            _func = func;
        }

        public override PermutationParserBranch<TContext, TToken, W> Add<V, W>(Parser<TContext, TToken, V> parser, Func<R, V, W> resultSelector)
            => Add(p => p.Add(parser), resultSelector);

        public override PermutationParserBranch<TContext, TToken, W> AddOptional<V, W>(Parser<TContext, TToken, V> parser, Func<V> defaultValueFactory, Func<R, V, W> resultSelector)
            => Add(p => p.AddOptional(parser, defaultValueFactory), resultSelector);

        private PermutationParserBranch<TContext, TToken, W> Add<V, W>(Func<PermutationParser<TContext, TToken, T>, PermutationParser<TContext, TToken, (T, V)>> addPerm, Func<R, V, W> resultSelector)
        {
            var this_func = _func;
            return new PermutationParserBranchImpl<TContext, TToken, U, (T, V), W>(
                _parser,
                addPerm(_perm),
                (tv, u) => resultSelector(this_func(tv.Item1, u), tv.Item2)
            );
        }

        public override Parser<TContext, TToken, R> Build()
        {
            var this_func = _func;
            return Parser.Map((x, y) => this_func(y, x), _parser, _perm.Build());
        }
    }
}
