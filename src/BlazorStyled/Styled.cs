using BlazorStyled.Internal;
using BlazorStyled.Internal.Components;
using BlazorStyled.Stylesheets;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public class Styled : ComponentBase, IDisposable
    {
        private readonly ServiceProvider _emptyServiceProvider = new ServiceCollection().BuildServiceProvider();
        private readonly Func<string, string> _encoder = (string t) => t;
        private string _previousClassname;

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Classname { get; set; }
        [Parameter] public MediaQueries MediaQuery { get; set; } = MediaQueries.None;
        [Parameter] public bool IsKeyframes { get; set; }
        [Parameter] public PseudoClasses PseudoClass { get; set; } = PseudoClasses.None;
        [Parameter] public EventCallback<string> ClassnameChanged { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> ComposeAttributes { get; set; }

        [Inject] private IStyled StyledService { get; set; }
        [Inject] private IStyleSheet StyleSheet { get; set; }


        protected override async Task OnParametersSetAsync()
        {
            await ProcessParameters();
        }

        protected async override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if(!StyleSheet.ScriptRendered)
            {
                if(await StyleSheet.BecomeScriptTag())
                {
                    builder.OpenComponent<Scripts>(1);
                    builder.CloseComponent();
                }
            }
        }

        private async Task ProcessParameters()
        {
            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            string classname = null;
            if (ComposeAttributes == null)
            {
                string content = RenderAsString();
                if (IsKeyframes)
                {
                    classname = styled.Keyframes(content);
                }
                else if (Classname != null && MediaQuery == MediaQueries.None && _previousClassname == null)
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
                else if (Classname != null && MediaQuery != MediaQueries.None && !ClassnameChanged.HasDelegate && _previousClassname == null)
                {
                    //Media query support for classes where an existing Classname already exists
                    content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                    styled.Css(GetMediaQuery(), content);
                }
                else if (Classname == null && PseudoClass == PseudoClasses.None && MediaQuery != MediaQueries.None && _previousClassname == null)
                {
                    //Media queries for html elements
                    styled.Css(GetMediaQuery(), content);
                }
                else if (Classname != null && PseudoClass != PseudoClasses.None && MediaQuery == MediaQueries.None && _previousClassname == null)
                {
                    content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                    styled.Css(content);
                }
                else
                {
                    classname = styled.Css(content);
                    /*if (_previousClassname == null)
                    {
                        classname = styled.Css(content);
                    }*/
                }
                await NotifyChanged(classname);
            }
            else
            {
                if (ClassnameChanged.HasDelegate)
                {
                    StringBuilder sb = new StringBuilder();
                    IList<string> labels = new List<string>();
                    IList<string> composeClasses = GetComposeClasses();
                    foreach (string cls in composeClasses)
                    {
                        string selector = ComposeAttributes[cls].ToString();
                        IList<IRule> rules = StyleSheet.GetRules(Id, selector);
                        if (rules != null)
                        {
                            foreach (IRule rule in rules)
                            {
                                if (rule.Selector != selector)
                                {
                                    string pseudo = rule.Selector.Replace("." + selector, "");
                                    sb.Append('&').Append(pseudo).Append('{');
                                }
                                foreach (Declaration decleration in rule.Declarations)
                                {
                                    sb.Append(decleration.ToString());
                                }
                                if (rule.Label != null)
                                {
                                    labels.Add(rule.Label);
                                }
                                if (rule.Selector != selector)
                                {
                                    sb.Append('}');
                                }
                            }
                        }
                    }
                    if (sb.Length != 0)
                    {
                        string css = sb.ToString();
                        if (labels.Count != 0)
                        {
                            string labelStr = string.Join("-", labels);
                            css = $"label:{labelStr};{css}";
                        }
                        classname = styled.Css(css);
                        await NotifyChanged(classname);
                    }
                }
            }
        }

        private IList<string> GetComposeClasses()
        {
            IList<string> ret = new List<string>();
            foreach (string key in ComposeAttributes.Keys)
            {
                if (key.ToLower().StartsWith("compose") && !key.ToLower().EndsWith("if"))
                {
                    if (ComposeAttributes[key] != null)
                    {
                        bool allowedToUse = true;
                        foreach (string innerKey in ComposeAttributes.Keys)
                        {
                            if (innerKey.ToLower() == $"{key}If".ToLower())
                            {
                                if (bool.TryParse(ComposeAttributes[innerKey].ToString(), out bool result) && !result)
                                {
                                    allowedToUse = false;
                                }
                            }
                        }
                        if (allowedToUse)
                        {
                            ret.Add(key);
                        }
                    }
                }
            }
            return ret;
        }

        private async Task NotifyChanged(string classname)
        {
            if (classname != null && ClassnameChanged.HasDelegate && _previousClassname != classname)
            {
                _previousClassname = classname;
                await ClassnameChanged.InvokeAsync(classname);
            }
        }

        private string ApplyPseudoClass(string classname)
        {
            string cls = classname.IndexOf("-") != -1 ? "." + classname : classname;
            return PseudoClass switch
            {
                PseudoClasses.Active => $"{cls}:active",
                PseudoClasses.After => $"{cls}::after",
                PseudoClasses.Before => $"{cls}::before",
                PseudoClasses.Checked => $"{cls}:checked",
                PseudoClasses.Disabled => $"{cls}:disabled",
                PseudoClasses.Empty => $"{cls}:empty",
                PseudoClasses.Enabled => $"{cls}:enabled",
                PseudoClasses.FirstChild => $"{cls}:first-child",
                PseudoClasses.FirstLetter => $"{cls}::first-letter",
                PseudoClasses.FirstLine => $"{cls}::first-line",
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
                PseudoClasses.Selection => $"{cls}::selection",
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
                IEnumerable<string> tokens = GetResult(htmlRenderer.Dispatcher.InvokeAsync(() => htmlRenderer.RenderComponentAsync<TempComponent>(paramView)));
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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _emptyServiceProvider.Dispose();
            }
        }

        private class TempComponent : ComponentBase
        {
            [Parameter] public RenderFragment ChildContent { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.AddContent(0, ChildContent);
            }
        }
    }
}