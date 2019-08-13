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
        [Parameter] private PseudoClasses PseudoClass { get; set; } = PseudoClasses.None;
        [Parameter] private EventCallback<string> ClassnameChanged { get; set; }

        [Inject] private IStyled StyledService { get; set; }

        protected override async Task OnInitAsync()
        {
            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            //TODO: below does not work well
            /*string classname = styled.Css("visibility: hidden;");
            if (ClassnameChanged.HasDelegate)
            {
                await ClassnameChanged.InvokeAsync(classname);
            }*/
            string content = RenderAsString();
            content = ApplyTheme(styled, content);
            string classname;
            if (IsKeyframes)
            {
                classname = styled.Keyframes(content);
                await NotifyChanged(classname);
            }
            else if(MediaQuery != MediaQueries.None)
            {
                if (ClassnameChanged.HasDelegate)
                {
                    //If ClassnameChanged has a delegate then @bind-Classname was used and this is a "new" style
                    //Otherwise Classname was used and this an existing style which will be handled in OnParametersSet
                    content = WrapWithMediaQuery(content);
                    classname = styled.Css(content);
                    await NotifyChanged(classname);
                }
            }
            else if(Classname != null)
            {
                //html elements
                styled.Css(ApplyPseudoClass(Classname), content);
            }
            else
            {
                //TODO: psudeo
                classname = styled.Css(content);
                await NotifyChanged(classname);
            }
        }

        protected override void OnParametersSet()
        {
            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            //Media Queries
            if (Classname != null && MediaQuery != MediaQueries.None && !ClassnameChanged.HasDelegate)
            {
                //Media query support for classes where an existing Classname already exists
                string content = RenderAsString();
                content = ApplyTheme(styled, content);
                content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                styled.Css(GetMediaQuery(), content);
            }
            else if (Classname == null && PseudoClass == PseudoClasses.None && MediaQuery != MediaQueries.None)
            {
                //Media queries for html elements
                string content = RenderAsString();
                content = ApplyTheme(styled, content);
                styled.Css(GetMediaQuery(), content);
            }
            else if(Classname != null && PseudoClass != PseudoClasses.None && MediaQuery == MediaQueries.None)
            {
                string content = RenderAsString();
                content = ApplyTheme(styled, content);
                content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                styled.Css(content);
            }
        }

        private async Task NotifyChanged(string classname)
        {
            if (classname != null && ClassnameChanged.HasDelegate)
            {
                await ClassnameChanged.InvokeAsync(classname);
            }
        }

        private string ApplyTheme(IStyled styled, string content)
        {
            Theme theme = styled.Theme;
            foreach (string key in theme.Values.Keys)
            {
                content = content.Replace("{" + key + "}", theme.Values[key]);
            }
            return content;
        }

        private string ApplyPseudoClass(string classname)
        {
            return PseudoClass switch
            {
                PseudoClasses.Active => $"{classname}:active",
                PseudoClasses.Checked => $"{classname}:checked",
                PseudoClasses.Disabled => $"{classname}:disabled",
                PseudoClasses.Empty => $"{classname}:empty",
                PseudoClasses.Enabled => $"{classname}:enabled",
                PseudoClasses.FirstChild => $"{classname}:first-child",
                PseudoClasses.FirstOfType => $"{classname}:first-of-type",
                PseudoClasses.Focus => $"{classname}:focus",
                PseudoClasses.Hover => $"{classname}:hover",
                PseudoClasses.InRange => $"{classname}:in-range",
                PseudoClasses.Invalid => $"{classname}:invalid",
                PseudoClasses.LastChild => $"{classname}:last-child",
                PseudoClasses.LastOfType => $"{classname}:last-of-type",
                PseudoClasses.Link => $"{classname}:link",
                PseudoClasses.Not => $":not{classname}",
                PseudoClasses.OnlyChild => $"{classname}:only-child",
                PseudoClasses.OnlyOfType => $"{classname}:only-of-type",
                PseudoClasses.Optional => $"{classname}:optional",
                PseudoClasses.OutOfRange => $"{classname}:out-of-range",
                PseudoClasses.ReadOnly => $"{classname}:read-only",
                PseudoClasses.ReadWrite => $"{classname}:read-write",
                PseudoClasses.Required => $"{classname}:required",
                PseudoClasses.Target => $"{classname}:target",
                PseudoClasses.Valid => $"{classname}:valid",
                PseudoClasses.Visited => $"{classname}:visited",
                _ => classname
            };
        }

        private string WrapWithMediaQuery(string content)
        {
            string query = GetMediaQuery();
            return $"{query}{{{content}}}";
        }

        private string WrapWithMediaQuery(string classname, string content)
        {
            //If classname includes a dash it is a classname, otherwise it is html elements
            if(classname.IndexOf('-') != -1) return $".{classname}{{{content}}}";
            return $"{classname}{{{content}}}";
        }

        private string GetMediaQuery()
        {
            return MediaQuery switch
            {
                MediaQueries.Mobile => "@media only screen and (max-width:480px)",
                MediaQueries.Tablet => "@media only screen and (max-width:768px)",
                MediaQueries.Default => "@media only screen and (max-width:980px)",
                MediaQueries.Large => "@media only screen and (max-width:1280px)",
                MediaQueries.Larger => "@media only screen and (max-width:1600px)",
                MediaQueries.LargerThanMobile => "@media only screen and (min-width:480px)",
                MediaQueries.LargerThanTablet => "@media only screen and (min-width:768px)",
                _ => string.Empty,
            };
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