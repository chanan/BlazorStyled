using BlazorStyled.Internal;
using System.Collections.Generic;

namespace BlazorStyled.Stylesheets
{
    internal class StyleSheetMetadata
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public IDictionary<string, IRule> Classes { get; set; } = new Dictionary<string, IRule>();
        public IDictionary<string, IDictionary<string, IRule>> Elements { get; set; } = new Dictionary<string, IDictionary<string, IRule>>();
        public Theme Theme { get; } = new Theme();
        public InternalGlobalStyles GlobalStyles = new InternalGlobalStyles();
    }
}
