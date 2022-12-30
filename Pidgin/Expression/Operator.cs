using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Pidgin.ParsingContext;

namespace Pidgin.Expression
{
    /// <summary>
    /// Methods to create <see cref="OperatorTableRow{TToken, T}"/> values.
    /// </summary>
    [SuppressMessage(
        "naming",
        "CA1716:Rename type so that it no longer conflicts with a reserved language keyword",
        Justification = "Would be a breaking change"
    )]
    public static class Operator
    {
        /// <summary>
        /// Creates a row in a table of operators which contains a single binary infix operator with the specified associativity.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="type">The associativity of the infix operator.</param>
        /// <param name="opParser">A parser for an infix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single infix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> Binary<TContext, TToken, T>(
            BinaryOperatorType type,
            Parser<TContext, TToken, Func<T, T, T>> opParser
        )
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return type switch
            {
                BinaryOperatorType.NonAssociative => InfixN(opParser),
                BinaryOperatorType.LeftAssociative => InfixL(opParser),
                BinaryOperatorType.RightAssociative => InfixR(opParser),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single unary operator - either a prefix operator or a postfix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="type">The type of the unary operator.</param>
        /// <param name="opParser">A parser for a unary operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single unary operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> Unary<TContext, TToken, T>(
            UnaryOperatorType type,
            Parser<TContext, TToken, Func<T, T>> opParser
        )
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return type switch
            {
                UnaryOperatorType.Prefix => Prefix(opParser),
                UnaryOperatorType.Postfix => Postfix(opParser),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single non-associative infix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="opParser">A parser for an infix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single infix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> InfixN<TContext, TToken, T>(Parser<TContext, TToken, Func<T, T, T>> opParser)
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return new OperatorTableRow<TContext, TToken, T>(new[] { opParser }, null, null, null, null);
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single left-associative infix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="opParser">A parser for an infix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single infix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> InfixL<TContext, TToken, T>(Parser<TContext, TToken, Func<T, T, T>> opParser)
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return new OperatorTableRow<TContext, TToken, T>(null, new[] { opParser }, null, null, null);
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single right-associative infix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="opParser">A parser for an infix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single infix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> InfixR<TContext, TToken, T>(Parser<TContext, TToken, Func<T, T, T>> opParser)
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return new OperatorTableRow<TContext, TToken, T>(null, null, new[] { opParser }, null, null);
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single prefix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="opParser">A parser for an prefix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single prefix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> Prefix<TContext, TToken, T>(Parser<TContext, TToken, Func<T, T>> opParser)
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return new OperatorTableRow<TContext, TToken, T>(null, null, null, new[] { opParser }, null);
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a single postfix operator.
        /// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
        /// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
        /// </summary>
        /// <param name="opParser">A parser for an postfix operator.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a single postfix operator.</returns>
        public static OperatorTableRow<TContext, TToken, T> Postfix<TContext, TToken, T>(Parser<TContext, TToken, Func<T, T>> opParser)
            where TContext : IParsingContext
        {
            if (opParser == null)
            {
                throw new ArgumentNullException(nameof(opParser));
            }

            return new OperatorTableRow<TContext, TToken, T>(null, null, null, null, new[] { opParser });
        }

        /// <summary>
        /// Creates a row in a table of operators which contains a chainable collection of prefix operators.
        /// By default <see cref="Prefix"/> operators can only appear once, so <c>- - 1</c> would not be parsed as "minus minus 1".
        ///
        /// This method is equivalent to:
        /// <code>
        /// Prefix(
        ///     OneOf(opParsers)
        ///         .AtLeastOnce()
        ///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.AggregateR(z, (f, x) => f(x)))
        /// )
        /// </code>
        /// </summary>
        /// <param name="opParsers">A collection of parsers for individual prefix operators.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a chainable collection of prefix operators.</returns>
        public static OperatorTableRow<TContext, TToken, T> PrefixChainable<TContext, TToken, T>(IEnumerable<Parser<TContext, TToken, Func<T, T>>> opParsers)
            where TContext : IParsingContext
            => Prefix(
                Parser.OneOf(opParsers)
                    .AtLeastOncePooled()
                    .Select<Func<T, T>>(fs =>
                        z =>
                        {
                            for (var i = fs.Count - 1; i >= 0; i--)
                            {
                                z = fs[i](z);
                            }

                            fs.Dispose();
                            return z;
                        }
                    )
            );

        /// <summary>
        /// Creates a row in a table of operators which contains a chainable collection of prefix operators.
        /// By default <see cref="Prefix"/> operators can only appear once, so <c>- - 1</c> would not be parsed as "minus minus 1".
        ///
        /// This method is equivalent to:
        /// <code>
        /// Prefix(
        ///     OneOf(opParsers)
        ///         .AtLeastOnce()
        ///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.AggregateR(z, (f, x) => f(x)))
        /// )
        /// </code>
        /// </summary>
        /// <param name="opParsers">A collection of parsers for individual prefix operators.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a chainable collection of prefix operators.</returns>
        public static OperatorTableRow<TContext, TToken, T> PrefixChainable<TContext, TToken, T>(params Parser<TContext, TToken, Func<T, T>>[] opParsers)
            where TContext : IParsingContext
            => PrefixChainable(opParsers.AsEnumerable());

        /// <summary>
        /// Creates a row in a table of operators which contains a chainable collection of postfix operators.
        /// By default <see cref="Postfix"/> operators can only appear once, so <c>foo()()</c> would not be parsed as "call(call(foo))".
        ///
        /// This method is equivalent to:
        /// <code>
        /// Postfix(
        ///     OneOf(opParsers)
        ///         .AtLeastOnce()
        ///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.Aggregate(z, (x, f) => f(x)))
        /// )
        /// </code>
        /// </summary>
        /// <param name="opParsers">A collection of parsers for individual postfix operators.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a chainable collection of postfix operators.</returns>
        public static OperatorTableRow<TContext, TToken, T> PostfixChainable<TContext, TToken, T>(IEnumerable<Parser<TContext, TToken, Func<T, T>>> opParsers)
            where TContext : IParsingContext
            => Postfix(
                Parser.OneOf(opParsers)
                    .AtLeastOncePooled()
                    .Select<Func<T, T>>(fs =>
                        z =>
                        {
                            for (var i = 0; i < fs.Count; i++)
                            {
                                z = fs[i](z);
                            }

                            fs.Dispose();
                            return z;
                        }
                    )
            );

        /// <summary>
        /// Creates a row in a table of operators which contains a chainable collection of postfix operators.
        /// By default <see cref="Postfix"/> operators can only appear once, so <c>foo()()</c> would not be parsed as "call(call(foo))".
        ///
        /// This method is equivalent to:
        /// <code>
        /// Postfix(
        ///     OneOf(opParsers)
        ///         .AtLeastOnce()
        ///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.Aggregate(z, (x, f) => f(x)))
        /// )
        /// </code>
        /// </summary>
        /// <param name="opParsers">A collection of parsers for individual postfix operators.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>A row in a table of operators which contains a chainable collection of postfix operators.</returns>
        public static OperatorTableRow<TContext, TToken, T> PostfixChainable<TContext, TToken, T>(params Parser<TContext, TToken, Func<T, T>>[] opParsers)
            where TContext : IParsingContext
            => PostfixChainable(opParsers.AsEnumerable());
    }
}
