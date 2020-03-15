using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorStyled.Internal
{
    internal class InternalGlobalStyles : IGlobalStyles
    {
        private IDictionary<string, string> _globalStyles = new Dictionary<string, string>();

        public string this[string globalClassName]
        {
            get
            {
                if(_globalStyles.ContainsKey(globalClassName))
                {
                    return _globalStyles[globalClassName];
                }
                else
                {
                    return String.Empty;
                }
            }
            
            set
            {
                if(_globalStyles.ContainsKey(globalClassName))
                {
                    _globalStyles[globalClassName] = value;
                }
                else
                {
                    _globalStyles.Add(globalClassName, value);
                }
            }
        }
    }
}
