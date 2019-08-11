using BlazorStyled.Stylesheets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class MediaQuery : BaseRule
    {
        public override RuleType RuleType => RuleType.MediaQuery;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Declarations.Count() > 0)
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
