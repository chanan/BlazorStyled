using BlazorStyled.Stylesheets;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class RuleSet : IRule
    {
        public string Selector { get; set; }
        public string Label { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.RuleSet;
        public List<IRule> NestedRules { get; set; } = new List<IRule>();
        private readonly Hash _hash = new Hash();

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

        public void SetClassName()
        {
            _hash.SetHashCode(this, Label);
        }
    }
}
