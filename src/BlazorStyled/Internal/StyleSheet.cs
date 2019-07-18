using System.Collections.Generic;
using System.Linq;

namespace BlazorStyled.Internal
{
    internal class StyleSheet
    {
        public List<IRule> Classes { get; } = new List<IRule>();

        public bool ClassExists(string selector)
        {
            return Classes.Where(c => c.Selector == selector).ToList().Count > 0;
        }
    }
}
