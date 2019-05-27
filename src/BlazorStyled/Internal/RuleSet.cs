using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    class RuleSet : IRule
    {
        public string Selector { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.RuleSet;
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('.').Append(Selector).Append('{');
            foreach(var rule in Declarations)
            {
                sb.Append(rule.ToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
        public List<IRule> NestedRules { get; set; } = new List<IRule>();
        public void SetClassName()
        {
            var hashs = new List<int>();
            foreach (var rule in Declarations)
            {
                hashs.Add(rule.GetHashCode());
            }
            foreach(var nestedRuleSet in NestedRules)
            {
                foreach (var rule in nestedRuleSet.Declarations)
                {
                    hashs.Add(rule.GetHashCode());
                }
            }
            hashs.Sort();
            uint hash = 0;
            foreach (int code in hashs)
            {
                unchecked
                {
                    hash *= 251;
                    hash += (uint)code;
                }
            }
            Selector = ConvertToBase64Arithmetic(hash);
            foreach (var nestedRuleSet in NestedRules)
            {
                nestedRuleSet.Selector = nestedRuleSet.Selector.Replace("&", "." + Selector);
            }
        }
        private string ConvertToBase64Arithmetic(uint i)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            uint length = (uint)alphabet.Length;
            var sb = new StringBuilder();
            var pos = 0;
            do
            {
                sb.Append(alphabet[(int)(i % length)]);
                i /= length;
                pos++;
                if (pos == 4)
                {
                    pos = 0;
                    if (i != 0) sb.Append('-');
                }
            } while (i != 0);
            return sb.ToString();
        }
    }
}
