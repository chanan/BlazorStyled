using BlazorStyled.Stylesheets;
using System;
using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal class ImportUri : BaseRule
    {
        private readonly string _uri;
        public override RuleType RuleType => RuleType.Import;
        public ImportUri(string uri)
        {
            _uri = uri;
            AddDeclaration(new Declaration { Property = "@import", Value = uri });
            Selector = Hash;
        }

        public override string ToString()
        {
            return $"@import url('{_uri}');";
        }
    }
}
