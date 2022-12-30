using System.Collections.Generic;

namespace Pidgin.ParsingContext
{
    public class ParsingContext : IParsingContext
    {
        public IDictionary<string, object> Items { get; set; }
    }
}
