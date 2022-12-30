using System;
using System.Collections.Generic;
using System.Linq;

namespace Pidgin
{
    public abstract partial class Parser<TContext, TToken, T>
    {
        /// <summary>
        /// Creates a parser which applies the current parser <paramref name="count"/> times.
        /// </summary>
        /// <param name="count">The number of times to apply the current parser.</param>
        /// <exception cref="InvalidOperationException"><paramref name="count"/> is less than 0.</exception>
        /// <returns>A parser which applies the current parser <paramref name="count"/> times.</returns>
        public Parser<TContext, TToken, IEnumerable<T>> Repeat(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative");
            }

            return Parser.Sequence<TContext, TToken>(Enumerable.Repeat(this, count));
        }
    }
}
