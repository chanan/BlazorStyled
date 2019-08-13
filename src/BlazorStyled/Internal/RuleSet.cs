using BlazorStyled.Stylesheets;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class RuleSet : BaseRule
    {
        public override RuleType RuleType => RuleType.RuleSet;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('.').Append(Selector).Append('{');
            foreach (Declaration rule in Declarations)
            {
                sb.Append(rule.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }

        public override void SetClassname()
        {
            SetHash();
            Selector = Hash;
            foreach (IRule nestedRuleSet in NestedRules)
            {
                if (nestedRuleSet.Label != null)
                {
                    Selector = Selector + '-' + nestedRuleSet.Label;
                }
            }
            foreach (IRule nestedRuleSet in NestedRules)
            {
                nestedRuleSet.Selector = nestedRuleSet.Selector.Replace("&", "." + Selector);
            }
        }
    }
}
