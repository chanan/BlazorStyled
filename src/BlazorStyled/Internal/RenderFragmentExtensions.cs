using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Text;

namespace BlazorStyled.Internal
{
    internal static class RenderFragmentExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "<Pending>")]
        internal static string RenderAsSimpleString(this RenderFragment childContent)
        {
            StringBuilder sb = new StringBuilder();
            RenderTreeBuilder builder = new RenderTreeBuilder();
            builder.AddContent(0, childContent);
            ArrayRange<RenderTreeFrame> array = builder.GetFrames();
            for (int i = 0; i < array.Count; i++)
            {
                RenderTreeFrame frame = array.Array[i];
                if (frame.FrameType == RenderTreeFrameType.Text)
                {
                    sb.Append(frame.TextContent);
                }
                if (frame.FrameType == RenderTreeFrameType.Markup)
                {
                    sb.Append(frame.MarkupContent);
                }
            }
            return sb.ToString();
        }
    }
}
