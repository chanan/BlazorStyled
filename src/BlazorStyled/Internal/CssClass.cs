using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    public class CssClass : ICloneable
    {
        public string Name { get; set; }
        public List<Rule> Rules { get; set; } = new List<Rule>();
        public bool IsPrimary { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Name != null)
            {
                if (IsPrimary) sb.Append('.');
                sb.Append(Name);
                sb.Append(' ');
            }
            sb.Append("{");
            foreach(var rule in Rules)
            {
                sb.Append(rule.ToString());
            }
            sb.Append("}");
            return sb.ToString();
        }

        public object Clone()
        {
            var ret = new CssClass { Name = Name, Rules = new List<Rule>() };
            foreach(var rule in Rules)
            {
                ret.Rules.Add((Rule)rule.Clone());
            }
            return ret;
        }
    }
}
