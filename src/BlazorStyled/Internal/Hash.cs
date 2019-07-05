using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    class Hash
    {
        public void GetHashCode(IRule ruleset, string label)
        {
            var hashs = new List<int>();
            if(ruleset.RuleType != RuleType.Keyframe)
            {
                foreach (var rule in ruleset.Declarations)
                {
                    hashs.Add(rule.GetHashCode());
                }
            }
            if(ruleset.RuleType != RuleType.FontFace)
            {
                foreach (var nestedRuleSet in ruleset.NestedRules)
                {
                    foreach (var rule in nestedRuleSet.Declarations)
                    {
                        hashs.Add(rule.GetHashCode());
                    }
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
            ruleset.Selector = label == null ? ConvertToBase64Arithmetic(hash) : ConvertToBase64Arithmetic(hash) + "-" + label;
            if (ruleset.RuleType != RuleType.FontFace)
            {
                foreach (var nestedRuleSet in ruleset.NestedRules)
                {
                    nestedRuleSet.Selector = nestedRuleSet.Selector.Replace("&", "." + ruleset.Selector);
                }
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
