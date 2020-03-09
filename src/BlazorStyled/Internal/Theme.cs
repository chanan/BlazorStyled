using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal class Theme
    {
        private IDictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public string AddOrUpdate(string key, string value)
        {
            string oldValue = null;
            if (!Values.ContainsKey(key))
            {
                Values.Add(key, value);
            }
            else if (Values[key] != value)
            {
                oldValue = Values[key];
                Values[key] = value;
            }
            return oldValue;
        }

        public IDictionary<string, string> GetTheme()
        {
            return new Dictionary<string, string>(Values);
        }
    }
}
