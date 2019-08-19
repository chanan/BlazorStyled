using System.Collections.Generic;

namespace BlazorStyled.Stylesheets
{
    public interface IRule
    {
        string Selector { get; set; }
        string ParentSelector { get; set; }
        string Label { get; set; }
        string Hash { get; }
        IEnumerable<Declaration> Declarations { get; }
        RuleType RuleType { get; }
        IEnumerable<IRule> NestedRules { get; }
        void SetHash();
        void AddDeclaration(Declaration declaration);
        void AddNestedRule(IRule rule);
        void AddDeclarations(IList<Declaration> declarations);
        void AddNestedRules(IList<IRule> rules);
        void SetClassname();
    }
}
