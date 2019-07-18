using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    internal class Styled : IStyled
    {
        private readonly StyledJsInterop _styledJsInterop;
        private readonly StyleSheet _styleSheet;

        public Styled(StyledJsInterop styledJsInterop, StyleSheet styleSheet)
        {
            _styledJsInterop = styledJsInterop;
            _styleSheet = styleSheet;
        }

        public Theme Theme { get; set; } = new Theme();

        public async Task<string> Css(string className, string css)
        {
            try
            {
                IRule rule;
                if (className.IndexOf("@font-face") != -1)
                {
                    rule = ParseFontFace(css);
                    await AddUniqueRuleSetToStyleSheet(rule);
                }
                else
                {
                    rule = ParsePredefinedRuleSet(className, css);
                    if (_elements.Contains(className))
                    {
                        await AddNonUniqueRuleSetToStyleSheet(rule);
                    }
                    else
                    {
                        await AddUniqueRuleSetToStyleSheet(rule);
                    }
                }
                return rule.Selector;
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public async Task<string> Css(string css)
        {
            try
            {
                RuleSet ruleSet = ParseRuleSet(css);
                await AddUniqueRuleSetToStyleSheet(ruleSet);
                foreach (IRule nestedRuleSet in ruleSet.NestedRules)
                {
                    await AddUniqueRuleSetToStyleSheet(nestedRuleSet);
                }
                return ruleSet.Selector;
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public async Task<string> Css(List<string> classes, string css)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cssClass in classes)
            {
                string result = await Css(cssClass, css);
                sb.Append(result).Append(' ');
            }
            return sb.ToString().Trim();
        }

        public async Task<string> Keyframes(string css)
        {
            try
            {
                Keyframe keyframe = ParseKeyframe(css);
                await AddUniqueRuleSetToStyleSheet(keyframe);
                return keyframe.Selector;
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        public async Task Fontface(string css)
        {
            try
            {
                FontFace fontface = ParseFontFace(css);
                await AddUniqueRuleSetToStyleSheet(fontface);
            }
            catch (StyledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw StyledException.GetException(css, e);
            }
        }

        private Keyframe ParseKeyframe(string css)
        {
            Keyframe keyframe = new Keyframe();
            IRule current = keyframe;
            string buffer = string.Empty;
            bool nestedClassClosed = true;
            foreach (char ch in css)
            {
                switch (ch)
                {
                    case ';':
                        Declaration declaration = ParseDeclaration(buffer.Trim());
                        if (declaration != null)
                        {
                            current.Declarations.Add(declaration);
                        }
                        buffer = string.Empty;
                        break;
                    case '{':
                        IRule nestedClass;
                        nestedClass = new PredefinedRuleSet
                        {
                            Selector = buffer.Trim()
                        };
                        keyframe.NestedRules.Add(nestedClass);
                        buffer = string.Empty;
                        current = nestedClass;
                        nestedClassClosed = false;
                        break;
                    case '}':
                        nestedClassClosed = true;
                        break;
                    default:
                        buffer += ch;
                        break;
                }
            }
            if (!nestedClassClosed)
            {
                throw StyledException.GetException(css, "A nested class is missing a '}' character", null);
            }
            if (buffer.Trim() != string.Empty)
            {
                throw StyledException.GetException(buffer, "This is usually caused by a missing ';' character at the end of a declaration", null);
            }
            keyframe.SetClassName();
            return keyframe;
        }

        private FontFace ParseFontFace(string css)
        {
            FontFace fontface = new FontFace
            {
                Declarations = ParseDeclarations(css)
            };
            fontface.SetClassName();
            return fontface;
        }

        private PredefinedRuleSet ParsePredefinedRuleSet(string className, string css)
        {
            PredefinedRuleSet predefinedRuleSet = new PredefinedRuleSet { Selector = className.Trim() };
            predefinedRuleSet.Declarations = ParseDeclarations(css);
            return predefinedRuleSet;
        }

        private List<Declaration> ParseDeclarations(string css)
        {
            List<Declaration> declarations = new List<Declaration>();
            string[] declarationsString = css.Trim().Split(';');
            foreach (string declarationString in declarationsString)
            {
                if (declarationString.IndexOf(':') != -1)
                {
                    Declaration declaration = ParseDeclaration(declarationString.Trim());
                    if (declaration != null)
                    {
                        declarations.Add(declaration);
                    }
                }
            }
            return declarations;
        }

        private RuleSet ParseRuleSet(string css)
        {
            RuleSet ruleSet = new RuleSet();
            IRule current = ruleSet;
            string buffer = string.Empty;
            bool nestedClassClosed = true; //Start from true becuase the parent doesnt need to be closed
            foreach (char ch in css)
            {
                switch (ch)
                {
                    case ';':
                        Declaration declaration = ParseDeclaration(buffer.Trim());
                        if (declaration != null)
                        {
                            if (declaration.Property == "label")
                            {
                                current.Label = declaration.Value.Trim();
                            }
                            else
                            {
                                current.Declarations.Add(declaration);
                            }
                        }
                        buffer = string.Empty;
                        break;
                    case '{':
                        IRule nestedClass;
                        if (buffer.IndexOf("@media") == -1)
                        {
                            nestedClass = new PredefinedRuleSet
                            {
                                Selector = buffer.Trim()
                            };
                        }
                        else
                        {
                            nestedClass = new MediaQuery
                            {
                                Selector = buffer.Trim() + "{&"
                            };
                        }
                        ruleSet.NestedRules.Add(nestedClass);
                        buffer = string.Empty;
                        current = nestedClass;
                        nestedClassClosed = false;
                        break;
                    case '}':
                        nestedClassClosed = true;
                        break;
                    default:
                        buffer += ch;
                        break;
                }
            }
            if (!nestedClassClosed)
            {
                throw StyledException.GetException(css, "A nested class is missing a '}' character", null);
            }
            if (buffer.Trim() != string.Empty)
            {
                throw StyledException.GetException(buffer, "This is usually caused by a missing ';' character at the end of a declaration", null);
            }
            ruleSet.SetClassName();
            return ruleSet;
        }

        private Declaration ParseDeclaration(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                string property = input.Substring(0, input.IndexOf(':'));
                string value = input.Substring(input.IndexOf(':') + 1);
                return new Declaration { Property = property, Value = value };
            }
            catch (Exception e)
            {
                throw StyledException.GetException(input, "This is likely cause by a missing ':' character", e);
            }
        }

        private async Task AddUniqueRuleSetToStyleSheet(IRule rule)
        {
            if (!_styleSheet.ClassExists(rule.Selector))
            {
                _styleSheet.Classes.Add(rule);
                await _styledJsInterop.InsertRule(rule.ToString());
            }
        }

        private async Task AddNonUniqueRuleSetToStyleSheet(IRule rule)
        {
            _styleSheet.Classes.Add(rule);
            await _styledJsInterop.InsertRule(rule.ToString());
        }

        private readonly List<string> _elements = new List<string>
        {
            "a",
            "abbr",
            "address",
            "area",
            "article",
            "aside",
            "audio",
            "b",
            "base",
            "bdi",
            "bdo",
            "blockquote",
            "body",
            "br",
            "button",
            "canvas",
            "caption",
            "cite",
            "code",
            "col",
            "colgroup",
            "data",
            "datalist",
            "dd",
            "del",
            "details",
            "dfn",
            "dialog",
            "div",
            "dl",
            "dt",
            "em",
            "embed",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "head",
            "header",
            "hr",
            "html",
            "i",
            "iframe",
            "img",
            "input",
            "ins",
            "kbd",
            "label",
            "legend",
            "li",
            "link",
            "main",
            "map",
            "mark",
            "meta",
            "meter",
            "nav",
            "noscript",
            "object",
            "ol",
            "optgroup",
            "option",
            "output",
            "p",
            "param",
            "picture",
            "pre",
            "progress",
            "q",
            "rp",
            "rt",
            "ruby",
            "s",
            "samp",
            "script",
            "section",
            "select",
            "small",
            "source",
            "span",
            "strong",
            "style",
            "sub",
            "summary",
            "sup",
            "svg",
            "table",
            "tbody",
            "td",
            "template",
            "textarea",
            "tfoot",
            "th",
            "thead",
            "time",
            "title",
            "tr",
            "track",
            "u",
            "ul",
            "var",
            "video",
            "wbr"
        };
    }
}
