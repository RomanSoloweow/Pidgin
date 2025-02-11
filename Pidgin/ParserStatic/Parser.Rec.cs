using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which lazily calls the supplied function and applies the resulting parser.
        /// This is primarily useful to allow mutual recursion in parsers.
        /// <seealso cref="Rec{TToken,T}(Lazy{Parser{TToken, T}})"/>
        /// <seealso cref="Rec{TToken,T}(Func{Parser{TToken, T},Parser{TToken, T}})"/>
        /// </summary>
        /// <param name="parser">A function which returns a parser.</param>
        /// <typeparam name="TToken">The type of tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The return type of the parser.</typeparam>
        /// <returns>A parser which lazily calls the supplied function and applies the resulting parser.</returns>
        /// <example>
        /// This example shows how to use mutual recursion to create a parser equivalent to <see cref="Parser{TToken, T}.Many()"/>.
        /// <code>
        /// // many is equivalent to String("foo").Separated(Char(' '))
        /// Parser&lt;char, string&gt; rest = null;
        /// var many = String("foo").Then(Rec(() => rest).Optional(), (x, y) => x + y.GetValueOrDefault(""));
        /// rest = Char(' ').Then(many);
        /// </code>
        /// </example>
        public static Parser<TContext, TToken, T> Rec<TContext, TToken, T>(Func<Parser<TContext, TToken, T>> parser)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            return Rec(new Lazy<Parser<TContext, TToken, T>>(parser));
        }

        /// <summary>
        /// Creates a parser which passes itself to the supplied function and applies the resulting parser.
        /// This is the Y combinator for parsers.
        /// <seealso cref="Rec{TToken,T}(Lazy{Parser{TToken, T}})"/>
        /// <seealso cref="Rec{TToken,T}(Func{Parser{TToken, T}})"/>
        /// </summary>
        /// <param name="func">A function whose argument is a parser which behaves the same way as its result.</param>
        /// <typeparam name="TToken">The type of tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The return type of the parser.</typeparam>
        /// <returns>A parser which lazily calls the supplied function and applies the resulting parser.</returns>
        /// <example>
        /// This example shows how to use mutual recursion to create a parser equivalent to <see cref="Parser{TToken, T}.Many()"/>.
        /// <code>
        /// // many is equivalent to String("foo").Separated(Char(' '))
        /// var many = Rec(self =>
        ///     String("foo").Then(
        ///         Char(' ').Then(self).Optional(),
        ///         (x, y) => x + y.GetValueOrDefault("")
        ///     )
        /// );
        /// </code>
        /// </example>
        public static Parser<TContext, TToken, T> Rec<TContext, TToken, T>(Func<Parser<TContext, TToken, T>, Parser<TContext, TToken, T>> func)
            where TContext : IParsingContext
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Parser<TContext, TToken, T> result = null!;
            result = Rec(() => func(result));
            return result;
        }

        /// <summary>
        /// Creates a parser which lazily calls the supplied function and applies the resulting parser.
        /// This is primarily useful to allow mutual recursion in parsers.
        /// <seealso cref="Rec{TToken,T}(Func{Parser{TToken, T}})"/>
        /// <seealso cref="Rec{TToken,T}(Func{Parser{TToken, T},Parser{TToken, T}})"/>
        /// </summary>
        /// <param name="parser">A lazy parser value.</param>
        /// <typeparam name="TToken">The type of tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The return type of the parser.</typeparam>
        /// <returns>A parser which lazily applies the specified parser.</returns>
        public static Parser<TContext, TToken, T> Rec<TContext, TToken, T>(Lazy<Parser<TContext, TToken, T>> parser)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            return new RecParser<TContext, TToken, T>(parser);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class RecParser<TContext, TToken, T> : Parser<TContext, TToken, T>
        where TContext : IParsingContext
    {
        private readonly Lazy<Parser<TContext, TToken, T>> _lazy;

        public RecParser(Lazy<Parser<TContext, TToken, T>> lazy)
        {
            _lazy = lazy;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result)
            => _lazy.Value.TryParse(ref context, ref state, ref expecteds, out result);
    }
}
