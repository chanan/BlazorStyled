using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class MediaQuery : IRule
    {
        public string Selector { get; set; }
        public string Label { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.MediaQuery;
        public List<IRule> NestedRules { get; set; } = new List<IRule>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Declarations.Count > 0)
            {
                sb.Append(Selector);
                sb.Append('{');
                foreach (Declaration rule in Declarations)
                {
                    sb.Append(rule.ToString());
                }
                sb.Append("}}");
            }
            else
            {
                sb.Append(Selector);
                sb.Append('{');
                foreach (IRule rule in NestedRules)
                {
                    sb.Append(rule.ToString());
                }
                sb.Append("}");
            }
            return sb.ToString();
        }
    }
}
