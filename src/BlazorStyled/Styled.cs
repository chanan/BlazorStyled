using BlazorStyled.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public class Styled : ComponentBase
    {
        private static readonly IDispatcher Dispatcher = Renderer.CreateDefaultDispatcher();
        private static readonly Func<string, string> Encoder = (t) => t;
        public readonly ServiceProvider EmptyServiceProvider = new ServiceCollection().BuildServiceProvider();

        [Parameter] private RenderFragment ChildContent { get; set; }
        [Parameter] private string Id { get; set; }
        [Parameter] private string Classname { get; set; }
        [Parameter] private MediaQueries MediaQuery { get; set; } = MediaQueries.None;
        [Parameter] private bool IsKeyframes { get; set; }
        [Parameter] private EventCallback<string> ClassnameChanged { get; set; }

        [Inject] private IStyled StyledService { get; set; }

        protected override async Task OnInitAsync()
        {
            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            string content = RenderAsString();
            content = ApplyTheme(styled, content);
            string classname;
            if (IsKeyframes)
            {
                classname = styled.Keyframes(content);
            }
            else
            {
                if (MediaQuery != MediaQueries.None)
                {
                    content = WrapWithMediaQuery(content);
                }
                classname = styled.Css(content);
            }
            if (ClassnameChanged.HasDelegate)
            {
                await ClassnameChanged.InvokeAsync(classname);
            }
        }

        private string ApplyTheme(IStyled styled, string content)
        {
            Theme theme = styled.Theme;
            foreach(var key in theme.Values.Keys)
            {
                content = content.Replace("{" + key + "}", theme.Values[key]);
            }
            return content;
        }

        private string WrapWithMediaQuery(string content)
        {
            string query = MediaQuery switch
            {
                MediaQueries.Mobile => "@media only screen and (max-width:480px)",
                MediaQueries.Tablet => "@media only screen and (max-width:768px)",
                MediaQueries.Default => "@media only screen and (max-width:980px)",
                MediaQueries.Large => "@media only screen and (max-width:1280px)",
                MediaQueries.Larger => "@media only screen and (max-width:1600px)",
                _ => string.Empty,
            };
            return $"{query}{{{content}}}";
        }

        private string RenderAsString()
        {
            string result = string.Empty;
            try
            {
                ParameterCollection paramCollection = ParameterCollection.FromDictionary(new Dictionary<string, object>() { { "ChildContent", ChildContent } });
                using HtmlRenderer htmlRenderer = new HtmlRenderer(EmptyServiceProvider, Encoder, Dispatcher);
                RenderTreeBuilder builder = new RenderTreeBuilder(htmlRenderer);
                builder.AddContent(0, ChildContent);
                IEnumerable<string> frames = from f in builder.GetFrames().Array
                                             where f.FrameType == RenderTreeFrameType.Markup || f.FrameType == RenderTreeFrameType.Text
                                             select f.MarkupContent;
                result = string.Join("", frames.ToList());
            }
            catch
            {
                //ignored dont crash if can't get result
            }
            return result;
        }
    }
}