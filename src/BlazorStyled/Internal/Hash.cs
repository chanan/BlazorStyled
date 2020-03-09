using BlazorStyled.Stylesheets;
using System.Collections.Generic;

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
                        hashs.Add(nestedRuleSet.Hash.GetStableHashCode());
                    }
                }
            }
            if (ruleset.RuleType == RuleType.MediaQuery || ruleset.RuleType == RuleType.PredefinedRuleSet)
            {
                hashs.Add(ruleset.Selector.GetStableHashCode());
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
            return label == null ? hash.ConvertToBase64Arithmetic() : hash.ConvertToBase64Arithmetic() + "-" + label;
        }
    }
}
