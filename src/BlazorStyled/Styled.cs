using BlazorStyled.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStyled
{
    public class Styled : IStyled
    {
        private readonly StyledJsInterop _styledJsInterop;
        private readonly StyleSheet _styleSheet;

        public Styled(StyledJsInterop styledJsInterop, StyleSheet styleSheet)
        {
            _styledJsInterop = styledJsInterop;
            _styleSheet = styleSheet;
        }

        public async Task<string> Css(string className, string css)
        {
            var classes = ParseCss(css);

            var primaryClass = (from cssClass in classes
                                where cssClass.Name == null
                                select cssClass).Single();

            if (className != null)
            {
                primaryClass.Name = className;
                primaryClass.IsPreDefinedName = true;
            }
            else
            {
                primaryClass.Name = GetClassName(classes);
            }
            primaryClass.IsPrimary = true;
            await AddClassToStyleSheet(primaryClass);

            var nestedClasses = (from cssClass in classes
                                 where cssClass.Name != null
                                 select cssClass).ToList();

            foreach (var cssClass in nestedClasses)
            {
                cssClass.Name = cssClass.Name.Replace("&", "." + primaryClass.Name);
                await AddClassToStyleSheet(cssClass);
            }

            return primaryClass.Name;
        }

        public Task<string> Css(string css)
        {
            return Css(null, css);
        }

        private List<CssClass> ParseCss(string css)
        {
            var cssClasses = new List<CssClass>();
            var cssClassesArr = css.Trim().Split('}');
            foreach (var cssClassStr in cssClassesArr)
            {
                var pair = cssClassStr.Trim().Split('{');
                var cssString = pair.Length == 1 ? pair[0] : pair[1];
                var cssClass = ParseRules(cssString);
                if (cssClass != null)
                {
                    if (pair.Length == 2)
                    {
                        if (pair[0].IndexOf(';') == -1) cssClass.Name = pair[0].Trim();
                        else
                        {
                            cssClass.Name = pair[0].Substring(pair[0].LastIndexOf(';') + 1).Trim();
                            var extraCss = pair[0].Substring(0, pair[0].LastIndexOf(';')).Trim();
                            var extraCssClass = ParseRules(extraCss);
                            if (extraCssClass != null) cssClasses.Add(extraCssClass);
                        }
                    }
                    cssClasses.Add(cssClass);
                }
            }
            return cssClasses;
        }

        private CssClass ParseRules(string css)
        {
            if (String.IsNullOrEmpty(css)) return null;
            var cssClass = new CssClass();
            var rules = css.Trim().Split(';');
            foreach (var rule in rules)
            {
                var pair = rule.Trim().Split(':');
                if (pair.Length == 2) cssClass.Rules.Add(new Rule { Name = pair[0].Trim(), Value = pair[1].Trim() });
            }
            return cssClass;
        }

        private async Task AddClassToStyleSheet(CssClass cssClass)
        {
            if (!_styleSheet.ClassExists(cssClass.Name))
            {
                _styleSheet.Classes.Add((CssClass)cssClass.Clone());
                await _styledJsInterop.InsertRule(cssClass.ToString());
            }
        }

        private string GetClassName(List<CssClass> cssClasses)
        {
            var hashs = new List<int>();
            foreach (var cssClass in cssClasses)
            {
                foreach (var rule in cssClass.Rules)
                {
                    hashs.Add(rule.GetHashCode());
                }
            }
            hashs.Sort();
            uint hash = 0;
            foreach (int code in hashs)
            {
                unchecked
                {
                    hash *= 251;
                    hash += (uint)code;
                }
            }
            var name = ConvertToBase64Arithmetic(hash);
            return name;
        }

        private string ConvertToBase64Arithmetic(uint i)
        {
            //const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            uint length = (uint)alphabet.Length;
            var sb = new StringBuilder();
            var pos = 0;
            do
            {
                sb.Append(alphabet[(int)(i % length)]);
                i /= length;
                pos++;
                if (pos == 4)
                {
                    pos = 0;
                    if (i != 0) sb.Append('-');
                }
            } while (i != 0);
            return sb.ToString();
        }
    }
}
