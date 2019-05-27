using System;
using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    class FontFace : IRule
    {
        public string Selector { get; set; }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public RuleType RuleType => RuleType.FontFace;
    }
}
