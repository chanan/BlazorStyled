using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public class Styled : ComponentBase
    {
        private readonly Func<string, string> _encoder = (string t) => t;
        public readonly ServiceProvider _emptyServiceProvider = new ServiceCollection().BuildServiceProvider();

        private string _previousClassname;

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Classname { get; set; }
        [Parameter] public MediaQueries MediaQuery { get; set; } = MediaQueries.None;
        [Parameter] public bool IsKeyframes { get; set; }
        [Parameter] public PseudoClasses PseudoClass { get; set; } = PseudoClasses.None;
        [Parameter] public EventCallback<string> ClassnameChanged { get; set; }

        [Inject] private IStyled StyledService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (_previousClassname != null)
            {
                return; //Prevent rentry
            }

            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            string content = RenderAsString();
            content = ApplyTheme(styled, content);
            string classname = null;
            if (IsKeyframes)
            {
                classname = styled.Keyframes(content);
            }
            else if (Classname != null && MediaQuery == MediaQueries.None)
            {
                //html elements
                styled.Css(ApplyPseudoClass(Classname), content);
            }
            else if (MediaQuery != MediaQueries.None && ClassnameChanged.HasDelegate)
            {
                //If ClassnameChanged has a delegate then @bind-Classname was used and this is a "new" style
                //Otherwise Classname was used and this an existing style which will be handled in OnParametersSet
                content = WrapWithMediaQuery(content);
                classname = styled.Css(content);
            }
            else if (Classname != null && MediaQuery != MediaQueries.None && !ClassnameChanged.HasDelegate)
            {
                //Media query support for classes where an existing Classname already exists
                content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                styled.Css(GetMediaQuery(), content);
            }
            else if (Classname == null && PseudoClass == PseudoClasses.None && MediaQuery != MediaQueries.None)
            {
                //Media queries for html elements
                styled.Css(GetMediaQuery(), content);
            }
            else if (Classname != null && PseudoClass != PseudoClasses.None && MediaQuery == MediaQueries.None)
            {
                content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                styled.Css(content);
            }
            else
            {
                classname = styled.Css(content);
            }
            await NotifyChanged(classname);
        }

        private async Task NotifyChanged(string classname)
        {
            if (classname != null && ClassnameChanged.HasDelegate && _previousClassname == null)
            {
                _previousClassname = classname;
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
            string cls = classname.IndexOf("-") != -1 ? "." + classname : classname;
            return PseudoClass switch
            {
                PseudoClasses.Active => $"{cls}:active",
                PseudoClasses.Checked => $"{cls}:checked",
                PseudoClasses.Disabled => $"{cls}:disabled",
                PseudoClasses.Empty => $"{cls}:empty",
                PseudoClasses.Enabled => $"{cls}:enabled",
                PseudoClasses.FirstChild => $"{cls}:first-child",
                PseudoClasses.FirstOfType => $"{cls}:first-of-type",
                PseudoClasses.Focus => $"{cls}:focus",
                PseudoClasses.Hover => $"{cls}:hover",
                PseudoClasses.InRange => $"{cls}:in-range",
                PseudoClasses.Invalid => $"{cls}:invalid",
                PseudoClasses.LastChild => $"{cls}:last-child",
                PseudoClasses.LastOfType => $"{cls}:last-of-type",
                PseudoClasses.Link => $"{cls}:link",
                PseudoClasses.Not => $":not{cls}",
                PseudoClasses.OnlyChild => $"{cls}:only-child",
                PseudoClasses.OnlyOfType => $"{cls}:only-of-type",
                PseudoClasses.Optional => $"{cls}:optional",
                PseudoClasses.OutOfRange => $"{cls}:out-of-range",
                PseudoClasses.ReadOnly => $"{cls}:read-only",
                PseudoClasses.ReadWrite => $"{cls}:read-write",
                PseudoClasses.Required => $"{cls}:required",
                PseudoClasses.Target => $"{cls}:target",
                PseudoClasses.Valid => $"{cls}:valid",
                PseudoClasses.Visited => $"{cls}:visited",
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
            if (classname.IndexOf('-') != -1)
            {
                return $".{classname}{{{content}}}";
            }

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
                ParameterView paramView = ParameterView.FromDictionary(new Dictionary<string, object>() { { "ChildContent", ChildContent } });
                using HtmlRenderer htmlRenderer = new HtmlRenderer(_emptyServiceProvider, NullLoggerFactory.Instance, _encoder);
                IEnumerable<string> tokens = GetResult(htmlRenderer.Dispatcher.InvokeAsync(() => htmlRenderer.RenderComponentAsync<TestComponent>(paramView)));
                result = string.Join("", tokens.ToArray());
            }
            catch
            {
                //ignored dont crash if can't get result
            }
            return result;
        }

        private IEnumerable<string> GetResult(Task<ComponentRenderedText> task)
        {
            if (task.IsCompleted && task.Status == TaskStatus.RanToCompletion && !task.IsFaulted && !task.IsCanceled)
            {
                return task.Result.Tokens;
            }
            else
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
                throw new InvalidOperationException("We will never hit this line");
            }
        }

        private class TestComponent : ComponentBase
        {
            [Parameter] public RenderFragment ChildContent { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.AddContent(0, ChildContent);
            }
        }
    }
}