using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    class FontFace : IRule
    {
        public string Selector { get; set; } = "@font-face";
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.FontFace;
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Selector).Append('{');
            foreach (var rule in Declarations)
            {
                sb.Append(rule.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
