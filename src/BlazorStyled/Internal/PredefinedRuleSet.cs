using BlazorStyled.Stylesheets;
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

        public void MergeDeceleration(Declaration declaration)
        {
            bool found = false;
            foreach (Declaration exsiting in Declarations)
            {
                if (exsiting.Property.Trim().ToLower() == declaration.Property.Trim().ToLower())
                {
                    found = true;
                    exsiting.Value = declaration.Value;
                    SetHash();
                }
            }
            if (!found)
            {
                AddDeclaration(declaration);
            }
        }
    }
}
