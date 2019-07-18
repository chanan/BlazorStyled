using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal interface IRule
    {
        string Selector { get; set; }
        string Label { get; set; }
        List<Declaration> Declarations { get; set; }
        RuleType RuleType { get; }
        List<IRule> NestedRules { get; set; }
    }
}
