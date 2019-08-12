using BlazorStyled.Stylesheets;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class FontFace : BaseRule
    {
        public override RuleType RuleType => RuleType.FontFace;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("@font-face").Append('{');
            foreach (Declaration rule in Declarations)
            {
                sb.Append(rule.ToString());
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
