using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BlazorStyled.Internal
{
    public class StyleSheet
    {
        public List<CssClass> Classes { get; } = new List<CssClass>();

        public bool ClassExists(string name) => Classes.Where(c => c.Name == name).SingleOrDefault() != null;
    }
}
