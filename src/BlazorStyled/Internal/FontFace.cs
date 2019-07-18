using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class FontFace : IRule
    {
        public string Selector { get; set; }
        public string Label { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.FontFace;
        public List<IRule> NestedRules { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private readonly Hash _hash = new Hash();

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

        public void SetClassName()
        {
            _hash.GetHashCode(this, null);
        }
    }
}
