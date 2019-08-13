using BlazorStyled.Stylesheets;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class Hash
    {
        public string GetHashCode(IRule ruleset, string label = null)
        {
            List<int> hashs = new List<int>();
            if (ruleset.RuleType != RuleType.Keyframe)
            {
                foreach (Declaration rule in ruleset.Declarations)
                {
                    hashs.Add(rule.GetHashCode());
                }
            }
            if (ruleset.RuleType != RuleType.FontFace && ruleset.RuleType != RuleType.PredefinedRuleSet && ruleset.RuleType != RuleType.Import)
            {
                foreach (IRule nestedRuleSet in ruleset.NestedRules)
                {
                    foreach (Declaration rule in nestedRuleSet.Declarations)
                    {
                        hashs.Add(rule.GetHashCode());
                    }
                    if (nestedRuleSet.RuleType == RuleType.MediaQuery || nestedRuleSet.RuleType == RuleType.PredefinedRuleSet)
                    {
                        hashs.Add(nestedRuleSet.Selector.GetHashCode());
                    }
                }
            }
            if(ruleset.RuleType == RuleType.MediaQuery || ruleset.RuleType == RuleType.PredefinedRuleSet)
            {
                hashs.Add(ruleset.Selector.GetHashCode());
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
            return label == null ? ConvertToBase64Arithmetic(hash) : ConvertToBase64Arithmetic(hash) + "-" + label;
        }

        private string ConvertToBase64Arithmetic(uint i)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            uint length = (uint)alphabet.Length;
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            do
            {
                sb.Append(alphabet[(int)(i % length)]);
                i /= length;
                pos++;
                if (pos == 4)
                {
                    pos = 0;
                    if (i != 0)
                    {
                        sb.Append('-');
                    }
                }
            } while (i != 0);
            return sb.ToString();
        }
    }
}
