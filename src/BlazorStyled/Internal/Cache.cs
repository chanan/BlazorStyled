using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal class Cache
    {
        public readonly IDictionary<string, string> Seen = new SortedDictionary<string, string>();
    }
}
