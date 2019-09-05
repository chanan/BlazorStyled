using System.Collections.Generic;

namespace BlazorStyled.Internal
{
    internal readonly struct ComponentRenderedText
    {
        internal ComponentRenderedText(int componentId, IEnumerable<string> tokens)
        {
            ComponentId = componentId;
            Tokens = tokens;
        }

        /// <summary>
        /// Gets the id associated with the component.
        /// </summary>
        public int ComponentId { get; }

        /// <summary>
        /// Gets the sequence of tokens that when concatenated represent the html for the rendered component.
        /// </summary>
        public IEnumerable<string> Tokens { get; }
    }
}
