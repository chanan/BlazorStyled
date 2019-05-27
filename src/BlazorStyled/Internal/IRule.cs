using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    interface IRule
    {
        string Selector { get; set; }
        List<Declaration> Declarations { get; set; }
        RuleType RuleType { get; }
        List<IRule> NestedRules { get; set; }
    }
}
