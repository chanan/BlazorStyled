using BlazorStyled.Internal;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public class Styled : ComponentBase
    {
        private string _previousClassname;

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Classname { get; set; }
        [Parameter] public MediaQueries MediaQuery { get; set; } = MediaQueries.None;
        [Parameter] public bool IsKeyframes { get; set; }
        [Parameter] public PseudoClasses PseudoClass { get; set; } = PseudoClasses.None;
        [Parameter] public EventCallback<string> ClassnameChanged { get; set; }
        [Parameter] public string GlobalStyle { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> ComposeAttributes { get; set; }

        [Inject] private IStyled StyledService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await ProcessParameters();
        }

        private async Task ProcessParameters()
        {
            IStyled styled = Id == null ? StyledService : StyledService.WithId(Id);
            string classname = null;

            string content = ChildContent.RenderAsSimpleString();
            if (content != null && content.Length > 0)
            {
                if (IsKeyframes)
                {
                    classname = await styled.Keyframes(content);
                }
                else if (Classname != null && MediaQuery == MediaQueries.None && _previousClassname == null)
                {
                    //html elements
                    await styled.Css(ApplyPseudoClass(Classname), content);
                }
                else if (MediaQuery != MediaQueries.None && ClassnameChanged.HasDelegate)
                {
                    //If ClassnameChanged has a delegate then @bind-Classname was used and this is a "new" style
                    //Otherwise Classname was used and this an existing style which will be handled below
                    content = WrapWithMediaQuery("&{" + content + "}");
                    classname = await styled.Css(content);
                }
                else if (Classname != null && MediaQuery != MediaQueries.None && !ClassnameChanged.HasDelegate && _previousClassname == null)
                {
                    //Media query support for classes where an existing Classname already exists
                    content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                    await styled.Css(GetMediaQuery(), content);
                }
                else if (Classname == null && PseudoClass == PseudoClasses.None && MediaQuery != MediaQueries.None && _previousClassname == null)
                {
                    //Media queries for html elements
                    await styled.Css(GetMediaQuery(), content);
                }
                else if (Classname != null && PseudoClass != PseudoClasses.None && MediaQuery == MediaQueries.None && _previousClassname == null)
                {
                    content = WrapWithMediaQuery(ApplyPseudoClass(Classname), content);
                    await styled.Css(content);
                }
                else
                {
                    if (PseudoClass == PseudoClasses.None && MediaQuery == MediaQueries.None)
                    {
                        classname = await styled.Css(content);
                    }
                }
                if (ComposeAttributes == null || !ClassnameChanged.HasDelegate)
                {
                    await NotifyChanged(classname);
                }
            }
            if (ComposeAttributes != null && ClassnameChanged.HasDelegate)
            {
                StringBuilder sb = new StringBuilder();
                if (classname != null)
                {
                    sb.Append(classname).Append(" ");
                }
                IList<string> composeClasses = GetComposeClasses();
                foreach (string cls in composeClasses)
                {
                    string selector = ComposeAttributes[cls].ToString();
                    sb.Append(selector).Append(" ");
                }
                if (sb.Length != 0)
                {
                    classname = sb.ToString().Trim();
                    await NotifyChanged(classname);
                }
            }
            if (GlobalStyle != null & classname != null)
            {
                await StyledService.SetGlobalStyle(GlobalStyle, classname);
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
    }
}