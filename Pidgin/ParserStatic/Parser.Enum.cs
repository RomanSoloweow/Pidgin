using System;
using System.Linq;

using Pidgin.ParsingContext;

namespace Pidgin
{
    public static partial class Parser
    {
        /// <summary>
        /// Creates a parser that parses and returns one of enum values.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A parser that parses and returns one of enum values.</returns>
        public static Parser<TContext, char, TEnum> Enum<TContext, TEnum>()
            where TContext : IParsingContext
            where TEnum : struct, Enum
        {
            return OneOf(System.Enum.GetNames<TEnum>()
                    .Select(String<TContext>)
                    .Select(Try))
                .Select(System.Enum.Parse<TEnum>)
                .Labelled("enum " + typeof(TEnum).Name);
        }

        /// <summary>
        /// Creates a parser that parses and returns one of enum values, in a case insensitive manner.
        /// </summary>
        /// <typeparam name="TEnum">Enum type.</typeparam>
        /// <returns>A parser that parses and returns one of enum values.</returns>
        public static Parser<TContext, char, TEnum> CIEnum<TContext, TEnum>()
            where TContext : IParsingContext
            where TEnum : struct, Enum
        {
            return OneOf(System.Enum.GetNames<TEnum>()
                    .Select(CIString<TContext>)
                    .Select(Try))
                .Select(x => System.Enum.Parse<TEnum>(x, true))
                .Labelled("enum " + typeof(TEnum).Name);
        }
    }
}
