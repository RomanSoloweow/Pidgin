using System;
using System.Collections.Generic;
using System.IO;

using Pidgin.Configuration;
using Pidgin.ParsingContext;
using Pidgin.TokenStreams;

using Config = Pidgin.Configuration.Configuration;

namespace Pidgin
{
    /// <summary>
    /// Extension methods for running parsers.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input string.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<char, T> Parse<TContext, T>(this Parser<TContext, char, T> parser, string input, IConfiguration<char>? configuration = null)
            where TContext : IParsingContext
            => Parse(parser, input.AsSpan(), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input list.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> Parse<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IList<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => DoParse(parser, new ListTokenStream<TToken>(input), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input list.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> ParseReadOnlyList<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IReadOnlyList<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => DoParse(parser, new ReadOnlyListTokenStream<TToken>(input), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input enumerable.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> Parse<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IEnumerable<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using var e = input.GetEnumerator();
            return Parse(parser, e, configuration);
        }

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input enumerator.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> Parse<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IEnumerator<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => DoParse(parser, new EnumeratorTokenStream<TToken>(input), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// Note that more characters may be consumed from <paramref name="input"/> than were required for parsing.
        /// You may need to manually rewind <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input stream.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<byte, T> Parse<TContext, T>(this Parser<TContext, byte, T> parser, Stream input, IConfiguration<byte>? configuration = null)
            where TContext : IParsingContext
            => DoParse(parser, new StreamTokenStream(input), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input reader.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<char, T> Parse<TContext, T>(this Parser<TContext, char, T> parser, TextReader input, IConfiguration<char>? configuration = null)
            where TContext : IParsingContext
            => DoParse(parser, new ReaderTokenStream(input), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input array.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> Parse<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, TToken[] input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => parser.Parse(input.AsSpan(), configuration);

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input span.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <returns>The result of parsing.</returns>
        public static Result<TToken, T> Parse<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, ReadOnlySpan<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            var state = new ParseState<TToken>(configuration ?? Config.Default<TToken>(), input);
            var result = DoParse(parser, ref state);
            return result;
        }

        private static Result<TToken, T> DoParse<TContext, TToken, T>(Parser<TContext, TToken, T> parser, ITokenStream<TToken> stream, IConfiguration<TToken>? configuration)
            where TContext : IParsingContext
        {
            var state = new ParseState<TToken>(configuration ?? Config.Default<TToken>(), stream);
            return DoParse(parser, ref state);
        }

        private static Result<TToken, T> DoParse<TContext, TToken, T>(Parser<TContext, TToken, T> parser, TContext context, ref ParseState<TToken> state)
            where TContext : IParsingContext
        {
            var expecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());

            var result1 = parser.TryParse(ref context, ref state, ref expecteds, out var result)
                ? new Result<TToken, T>(result)
                : new Result<TToken, T>(state.BuildError(ref expecteds));

            expecteds.Dispose();
            state.Dispose();  // ensure we return the state's buffers to the buffer pool

            return result1;
        }

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input string.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, T>(this Parser<TContext, char, T> parser, string input, IConfiguration<char>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input list.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IList<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input list.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseReadOnlyListOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IReadOnlyList<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.ParseReadOnlyList(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input enumerable.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IEnumerable<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input enumerator.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, IEnumerator<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input stream.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, T>(this Parser<TContext, byte, T> parser, Stream input, IConfiguration<byte>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input reader.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, T>(this Parser<TContext, char, T> parser, TextReader input, IConfiguration<char>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input array.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, TToken[] input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        /// <summary>
        /// Applies <paramref name="parser"/> to <paramref name="input"/>.
        /// </summary>
        /// <param name="parser">A parser.</param>
        /// <param name="input">An input span.</param>
        /// <param name="configuration">The configuration, or null to use the default configuration.</param>
        /// <typeparam name="TToken">The type of the tokens in the parser's input stream.</typeparam>
        /// <typeparam name="T">The type of the value returned by the parser.</typeparam>
        /// <typeparam name="TContext">The type of the parsing context used by the parser.</typeparam>
        /// <exception cref="ParseException">Thrown when an error occurs during parsing.</exception>
        /// <returns>The result of parsing.</returns>
        public static T ParseOrThrow<TContext, TToken, T>(this Parser<TContext, TToken, T> parser, ReadOnlySpan<TToken> input, IConfiguration<TToken>? configuration = null)
            where TContext : IParsingContext
            => GetValueOrThrow(parser.Parse(input, configuration));

        private static T GetValueOrThrow<TToken, T>(Result<TToken, T> result)
            => result.Success ? result.Value : throw new ParseException(result.Error!.RenderErrorMessage());
    }
}
