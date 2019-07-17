using System.Collections.Generic;
using System.Linq;

namespace BlazorStyled.Internal
{
    class StyleSheet
    {
        public List<IRule> Classes { get; } = new List<IRule>();

        public bool ClassExists(string selector) => Classes.Where(c => c.Selector == selector).ToList().Count > 0;
    }
}
