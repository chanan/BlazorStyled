using BlazorStyled.Stylesheets;
using System;
using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal class ImportUri : IRule
    {
        private readonly string _uri;
        private readonly Hash _hash = new Hash();

        public ImportUri(string uri)
        {
            _uri = uri;
            Declarations.Add(new Declaration { Property = "@import", Value = uri });
            Selector = _hash.GetHashCode(this, null);
        }
        public string Selector { get; set; }
        public string Label { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();

        public RuleType RuleType => RuleType.Import;

        public List<IRule> NestedRules { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ToString()
        {
            return $"@import url('{_uri}');";
        }
    }
}
