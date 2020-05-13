using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    class Cache
    {
        public readonly IDictionary<string, string> Seen = new SortedDictionary<string, string>();
    }
}
