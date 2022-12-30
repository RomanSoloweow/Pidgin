namespace Pidgin
{
    public abstract partial class Parser<TContext, TToken, T>
    {
        private static Parser<TContext, TToken, Maybe<T>>? _returnNothing;

        private static Parser<TContext, TToken, Maybe<T>> ReturnNothing
        {
            get
            {
                if (_returnNothing == null)
                {
                    _returnNothing = Parser.Return<TContext, TToken, Maybe<T>>(Maybe.Nothing<T>());
                }

                return _returnNothing;
            }
        }

        /// <summary>
        /// Creates a parser which applies the current parser and returns <see cref="Maybe.Nothing{T}()"/> if the current parser fails without consuming any input.
        /// The resulting parser fails if the current parser fails after consuming input.
        /// </summary>
        /// <returns>A parser which applies the current parser and returns <see cref="Maybe.Nothing{T}()"/> if the current parser fails without consuming any input.</returns>
        public Parser<TContext, TToken, Maybe<T>> Optional()
            => Select(Maybe.Just).Or(ReturnNothing);
    }
}
