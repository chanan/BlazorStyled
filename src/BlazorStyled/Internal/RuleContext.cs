using BlazorStyled.Stylesheets;
using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal class RuleContext
    {
        public StyleSheetMetadata Stylesheet { get; set; }
        public IRule Rule { get; set; }
        public KeyValuePair<string, string> ThemeEntry { get; set; }
        public string OldThemeValue { get; set; }
        public RuleContextEvent Event { get; set; }
    }
}