using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which applies <paramref name="parser"/> <paramref name="count"/> times,
        /// packing the resulting <c>char</c>s into a <c>string</c>.
        /// 
        /// <para>
        /// Equivalent to <c>parser.Repeat(count).Select(string.Concat)</c>.
        /// </para>
        /// </summary>
        /// <typeparam name="TToken">The type of tokens in the parser's input stream.</typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="parser">The parser.</param>
        /// <param name="count">The number of times to apply the parser.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> was less than 0.</exception>
        /// <returns>
        /// A parser which applies <paramref name="parser"/> <paramref name="count"/> times,
        /// packing the resulting <c>char</c>s into a <c>string</c>.
        /// </returns>
        public static Parser<TContext, TToken, string> RepeatString<TContext, TToken>(this Parser<TContext, TToken, char> parser, int count)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative");
            }

            return new RepeatStringParser<TContext, TToken>(parser, count);
        }
    }

    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:FileMayOnlyContainASingleType",
        Justification = "This class belongs next to the accompanying API method"
    )]
    internal sealed class RepeatStringParser<TContext, TToken> : Parser<TContext, TToken, string>
        where TContext : IParsingContext
    {
        private readonly Parser<TContext, TToken, char> _parser;
        private readonly int _count;

        public RepeatStringParser(Parser<TContext, TToken, char> parser, int count)
        {
            _parser = parser;
            _count = count;
        }

        public sealed override bool TryParse(ref TContext context, ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out string result)
        {
            var builder = new InplaceStringBuilder(_count);

            for (var i = 0; i < _count; i++)
            {
                var success = _parser.TryParse(ref context, ref state, ref expecteds, out var result1);

                if (!success)
                {
                    result = null;
                    return false;
                }

                builder.Append(result1);
            }

            result = builder.ToString();
            return true;
        }
    }
}
