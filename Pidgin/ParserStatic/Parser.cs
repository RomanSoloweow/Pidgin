using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Pidgin.ParsingContext;

namespace Pidgin
{
    /// <summary>
    /// Constructor functions, extension methods and utilities for <see cref="Parser{TToken, T}"/>.
    /// This class is intended to be imported statically ("using static Pidgin.Parser").
    /// </summary>
    public static partial class Parser
    {
    }

    /// <summary>
    /// Constructor functions, extension methods and utilities for <see cref="Parser{TToken, T}"/>
    /// This class is intended to be imported statically, with the type parameter set to the type of tokens in your input stream ("using static Pidgin.Parser&lt;char&gt;").
    /// </summary>
    [SuppressMessage(
        "design",
        "CA1000:Do not declare static members on generic types",
        Justification = "This type is designed to be imported statically"
    )]
    public static partial class Parser<TContext>
    {
    }
}
