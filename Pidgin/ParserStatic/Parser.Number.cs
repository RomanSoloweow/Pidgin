using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        private static Parser<TContext, char, string> SignString<TContext>()
            where TContext : IParsingContext
            => Char<TContext>('-').ThenReturn("-")
                .Or(Char<TContext>('+').ThenReturn("+"))
                .Or(Parser.Return<TContext, char, string>(""));

        private static Parser<TContext, char, int> Sign<TContext>()
        where TContext : IParsingContext
            => Char<TContext>('+').ThenReturn(1)
                .Or(Char<TContext>('-').ThenReturn(-1))
                .Or(Parser.Return<TContext, char, int>(1));

        /// <summary>
        /// A parser which parses a base-10 integer with an optional sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <returns>A parser which parses a base-10 integer with an optional sign.</returns>
        public static Parser<TContext, char, int> DecimalNum<TContext>()
            where TContext : IParsingContext
            => Int<TContext>(10).Labelled("number");

        /// <summary>
        /// A parser which parses a base-10 integer with an optional sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <returns>A parser which parses a base-10 integer with an optional sign.</returns>
        public static Parser<TContext, char, int> Num<TContext>()
            where TContext : IParsingContext
            => DecimalNum<TContext>();

        /// <summary>
        /// A parser which parses a base-10 long integer with an optional sign.
        /// </summary>
        public static Parser<TContext, char, long> LongNum<TContext>()
            where TContext : IParsingContext
            => Long<TContext>(10).Labelled("number");

        /// <summary>
        /// A parser which parses a base-8 (octal) integer with an optional sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <returns>A parser which parses a base-8 (octal) integer with an optional sign.</returns>
        public static Parser<TContext, char, int> OctalNum<TContext>()
            where TContext : IParsingContext
            => Int<TContext>(8).Labelled("octal number");

        /// <summary>
        /// A parser which parses a base-16 (hexadecimal) integer with an optional sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <returns>A parser which parses a base-16 (hexadecimal) integer with an optional sign.</returns>
        public static Parser<TContext, char, int> HexNum<TContext>() 
            where TContext : IParsingContext
            => Int<TContext>(16).Labelled("hexadecimal number");

        /// <summary>
        /// A parser which parses an integer in the given base with an optional sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <param name="base">The base in which the number is notated, between 1 and 36.</param>
        /// <returns>A parser which parses an integer with an optional sign.</returns>
        [SuppressMessage("design", "CA1720:Identifier contains type name", Justification = "Would be a breaking change")]
        public static Parser<TContext, char, int> Int<TContext>(int @base)
            where TContext : IParsingContext
            => Map(
                (sign, num) => sign * num,
                _sign,
                UnsignedInt<TContext>(@base)
            ).Labelled($"base-{@base} number");

        /// <summary>
        /// A parser which parses an integer in the given base without a sign.
        /// The resulting <c>int</c> is not checked for overflow.
        /// </summary>
        /// <param name="base">The base in which the number is notated, between 1 and 36.</param>
        /// <returns>A parser which parses an integer without a sign.</returns>
        public static Parser<TContext, char, int> UnsignedInt<TContext>(int @base)
            where TContext : IParsingContext
            => DigitChar<TContext>(@base)
                .ChainAtLeastOnce<int, IntChainer>(c => new IntChainer(@base))
                .Labelled($"base-{@base} number");

        private struct IntChainer : IChainer<int, int>
        {
            private readonly int _base;
            private int _result;

            public IntChainer(int @base)
            {
                _base = @base;
                _result = 0;
            }

            public void Apply(int value)
            {
                _result = (_result * _base) + value;
            }

            public int GetResult() => _result;

            public void OnError()
            {
            }
        }

        /// <summary>
        /// Creates a parser which parses a long integer in the given base with an optional sign.
        /// The resulting <see cref="long" /> is not checked for overflow.
        /// </summary>
        /// <param name="base">The base in which the number is notated, between 1 and 36.</param>
        /// <returns>A parser which parses a long integer with an optional sign.</returns>
        [SuppressMessage("design", "CA1720:Identifier contains type name", Justification = "Would be a breaking change")]
        public static Parser<TContext, char, long> Long<TContext>(int @base)
            where TContext : IParsingContext
            => Map(
                (sign, num) => sign * num,
                _sign,
                UnsignedLong<TContext>(@base)
            ).Labelled($"base-{@base} number");

        /// <summary>
        /// A parser which parses a long integer in the given base without a sign.
        /// The resulting <see cref="long" /> is not checked for overflow.
        /// </summary>
        /// <param name="base">The base in which the number is notated, between 1 and 36.</param>
        /// <returns>A parser which parses a long integer without a sign.</returns>
        public static Parser<TContext, char, long> UnsignedLong<TContext>(int @base)
            where TContext : IParsingContext
            => DigitCharLong<TContext>(@base)
                .ChainAtLeastOnce<long, LongChainer>(c => new LongChainer(@base))
                .Labelled($"base-{@base} number");

        private struct LongChainer : IChainer<long, long>
        {
            private readonly int _base;
            private long _result;

            public LongChainer(int @base)
            {
                _base = @base;
                _result = 0;
            }

            public void Apply(long value)
            {
                _result = (_result * _base) + value;
            }

            public long GetResult() => _result;

            public void OnError()
            {
            }
        }

        private static Parser<TContext, char, int> DigitChar<TContext>(int @base)
            where TContext : IParsingContext
            => @base <= 10
                ? Token<TContext, char>(c => c >= '0' && c < '0' + @base)
                    .Select(GetDigitValue)
                : Token<TContext, char>(c =>
                        c is >= '0' and <= '9'
                        || (c >= 'A' && c < 'A' + @base - 10)
                        || (c >= 'a' && c < 'a' + @base - 10)
                    )
                    .Select(GetLetterOrDigitValue);

        private static Parser<TContext, char, long> DigitCharLong<TContext>(int @base)
            where TContext : IParsingContext
            => @base <= 10
                ? Parser.Token<TContext, char>(c => c >= '0' && c < '0' + @base)
                    .Select(GetDigitValueLong)
                : Parser
                    .Token<TContext, char>(c =>
                        (c >= '0' && c <= '9')
                        || (c >= 'A' && c < 'A' + @base - 10)
                        || (c >= 'a' && c < 'a' + @base - 10)
                    )
                    .Select(c => GetLetterOrDigitValueLong(c));

        private static int GetDigitValue(char c) => c - '0';

        private static int GetLetterOrDigitValue(char c)
        {
            if (c is >= '0' and <= '9')
            {
                return GetDigitValue(c);
            }

            if (c is >= 'A' and <= 'Z')
            {
                return GetUpperLetterOffset(c) + 10;
            }

            return GetLowerLetterOffset(c) + 10;
        }

        private static int GetUpperLetterOffset(char c) => c - 'A';

        private static int GetLowerLetterOffset(char c) => c - 'a';

        private static long GetDigitValueLong(char c) => c - '0';

        private static long GetLetterOrDigitValueLong(char c)
        {
            if (c is >= '0' and <= '9')
            {
                return GetDigitValueLong(c);
            }

            if (c is >= 'A' and <= 'Z')
            {
                return GetUpperLetterOffsetLong(c) + 10;
            }

            return GetLowerLetterOffsetLong(c) + 10;
        }

        private static long GetUpperLetterOffsetLong(char c) => c - 'A';

        private static long GetLowerLetterOffsetLong(char c) => c - 'a';

        private static Parser<TContext, char, Unit> _fractionalPart<TContext>()
            where TContext : IParsingContext
            => Char<TContext>('.').Then(Digit<TContext>().SkipAtLeastOnce());

        private static Parser<TContext, char, Unit> OptionalFractionalPart<TContext>()
            where TContext : IParsingContext
            => _fractionalPart<TContext>().Or(Parser.Return<TContext, char, Unit>(Unit.Value));

        /// <summary>
        /// A parser which parses a floating point number with an optional sign.
        /// </summary>
        public static Parser<TContext, char, double> Real<TContext>()
            where TContext : IParsingContext
            => SignString<TContext>()
                .Then(
                    _fractionalPart<TContext>()
                        .Or(Digit<TContext>().SkipAtLeastOnce().Then(_optionalFractionalPart)) // if we saw an integral part, the fractional part is optional
                )
                .Then(
                    CIChar<TContext>('e').Then(SignString<TContext>()).Then(Digit<TContext>().SkipAtLeastOnce())
                        .Or(Parser.Return<TContext, char, Unit>(Unit.Value))
                )
                .MapWithInput((span, _) =>
                {
                    var success = double.TryParse(span.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
                    if (success)
                    {
                        return (double?)result;
                    }

                    return (double?)null;
                })
                .Assert(x => x.HasValue, "Couldn't parse a double")
                .Select(x => x!.Value)
                .Labelled($"real number");
    }
}
