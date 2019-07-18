using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class PredefinedRuleSet : IRule
    {
        public string Selector { get; set; }
        public string Label { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.PredefinedRuleSet;
        public List<IRule> NestedRules { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Selector);
            sb.Append('{');
            foreach (Declaration rule in Declarations)
            {
                sb.Append(rule.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
