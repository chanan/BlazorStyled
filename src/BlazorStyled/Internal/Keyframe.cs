using BlazorStyled.Stylesheets;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class Keyframe : BaseRule
    {
        public override RuleType RuleType => RuleType.Keyframe;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("@keyframes ").Append(Selector).Append('{');
            foreach (IRule nestedRule in NestedRules)
            {
                sb.Append(nestedRule.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }

        public virtual void SetClassName()
        {
            Selector = Hash;
        }
    }
}
