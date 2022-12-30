using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser which parses and returns a single character.
        /// </summary>
        /// <param name="character">The character to parse.</param>
        /// <returns>A parser which parses and returns a single character.</returns>
        [SuppressMessage(
            "design",
            "CA1720:Identifier contains type name",
            Justification = "Would be a breaking change"
        )]
        public static Parser<TContext, char, char> Char<TContext>(char character)
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(character);

        /// <summary>
        /// Creates a parser which parses and returns a single character, in a case insensitive manner.
        /// The parser returns the actual character parsed.
        /// </summary>
        /// <param name="character">The character to parse.</param>
        /// <returns>A parser which parses and returns a single character.</returns>
        public static Parser<TContext, char, char> CIChar<TContext>(char character)
            where TContext : IParsingContext
        {
            var theChar = char.ToLowerInvariant(character);
            var expected = ImmutableArray.Create(
                new Expected<char>(ImmutableArray.Create(char.ToUpperInvariant(character))),
                new Expected<char>(ImmutableArray.Create(char.ToLowerInvariant(character)))
            );
            return Parser.Token<TContext, char>(c => char.ToLowerInvariant(c) == theChar)
                .WithExpected(expected);
        }

        /// <summary>
        /// Creates a parser which parses and returns a character if it is not one of the specified characters.
        /// When the character is one of the given characters, the parser fails without consuming input.
        /// </summary>
        /// <param name="chars">A sequence of characters that should not be matched.</param>
        /// <returns>A parser which parses and returns a character that does not match one of the specified characters.</returns>
        public static Parser<TContext, char, char> AnyCharExcept<TContext>(params char[] chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            return AnyCharExcept<TContext>(chars.AsEnumerable());
        }

        /// <summary>
        /// Creates a parser which parses and returns a character if it is not one of the specified characters.
        /// When the character is one of the given characters, the parser fails without consuming input.
        /// </summary>
        /// <param name="chars">A sequence of characters that should not be matched.</param>
        /// <returns>A parser which parses and returns a character that does not match one of the specified characters.</returns>
        public static Parser<TContext, char, char> AnyCharExcept<TContext>(IEnumerable<char> chars)
            where TContext : IParsingContext
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            var cs = chars.ToArray();
            return Parser.Token<TContext, char>(c => Array.IndexOf(cs, c) == -1);
        }

        /// <summary>
        /// A parser that parses and returns a single digit character (0-9).
        /// </summary>
        /// <returns>A parser that parses and returns a single digit character.</returns>
        public static Parser<TContext, char, char> Digit<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsDigit).Labelled("digit");

        /// <summary>
        /// A parser that parses and returns a single letter character.
        /// </summary>
        public static Parser<TContext, char, char> Letter<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsLetter).Labelled("letter");

        /// <summary>
        /// A parser that parses and returns a single letter or digit character.
        /// </summary>
        public static Parser<TContext, char, char> LetterOrDigit<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsLetterOrDigit).Labelled("letter or digit");

        /// <summary>
        /// A parser that parses and returns a single lowercase letter character.
        /// </summary>
        public static Parser<TContext, char, char> Lowercase<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsLower).Labelled("lowercase letter");

        /// <summary>
        /// A parser that parses and returns a single uppercase letter character.
        /// </summary>
        public static Parser<TContext, char, char> Uppercase<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsUpper).Labelled("uppercase letter");

        /// <summary>
        /// A parser that parses and returns a single Unicode punctuation character.
        /// </summary>
        public static Parser<TContext, char, char> Punctuation<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsPunctuation).Labelled("punctuation");

        /// <summary>
        /// A parser that parses and returns a single Unicode symbol character.
        /// </summary>
        public static Parser<TContext, char, char> Symbol<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsSymbol).Labelled("symbol");

        /// <summary>
        /// A parser that parses and returns a single Unicode separator character.
        /// </summary>
        public static Parser<TContext, char, char> Separator<TContext>()
            where TContext : IParsingContext
            => Parser.Token<TContext, char>(char.IsSeparator).Labelled("separator");
    }
}
