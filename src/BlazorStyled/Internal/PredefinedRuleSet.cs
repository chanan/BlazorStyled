using BlazorStyled.Stylesheets;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class PredefinedRuleSet : BaseRule
    {
        public override RuleType RuleType => RuleType.PredefinedRuleSet;

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
