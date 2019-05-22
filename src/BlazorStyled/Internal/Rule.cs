using System;

namespace BlazorStyled.Internal
{
    public class Rule : ICloneable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{this.Name}: {this.Value};";
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public object Clone()
        {
            return new Rule { Name = this.Name, Value = this.Value };
        }
    }
}
