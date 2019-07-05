using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    class Keyframe : IRule
    {
        public string Selector { get; set; }
        public string Label { get; set; }
        public List<Declaration> Declarations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RuleType RuleType => RuleType.Keyframe;
        public List<IRule> NestedRules { get; set; } = new List<IRule>();
        private readonly Hash _hash = new Hash();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("@keyframes ").Append(Selector).Append('{');
            foreach (var nestedRule in NestedRules)
            {
                sb.Append(nestedRule.ToString());
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
