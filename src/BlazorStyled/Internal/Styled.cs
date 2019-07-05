using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    class Styled : IStyled
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
                    await AddUniqueRuleSetToStyleSheet(rule);
                }
                return rule.Selector;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in CSS string:");
                Console.Error.WriteLine(css);
                throw new Exception("Parse Error", e);
            }
        }

        public async Task<string> Css(string css)
        {
            try
            {
                RuleSet ruleSet = ParseRuleSet(css);
                await AddUniqueRuleSetToStyleSheet(ruleSet);
                foreach (var nestedRuleSet in ruleSet.NestedRules)
                {
                    await AddUniqueRuleSetToStyleSheet(nestedRuleSet);
                }
                return ruleSet.Selector;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in CSS string:");
                Console.Error.WriteLine(css);
                throw new Exception("Parse Error", e);
            }
        }

        /*public async Task<string> CssWithLabel(string label, string css)
        {
            try
            {
                RuleSet ruleSet = ParseRuleSet(css);
                await AddUniqueRuleSetToStyleSheet(ruleSet);
                foreach (var nestedRuleSet in ruleSet.NestedRules)
                {
                    await AddUniqueRuleSetToStyleSheet(nestedRuleSet);
                }
                return ruleSet.Selector;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in CSS string:");
                Console.Error.WriteLine(css);
                throw new Exception("Parse Error", e);
            }
        }*/

        public async Task<string> Keyframes(string css)
        {
            try
            {
                var keyframe = ParseKeyframe(css);
                await AddUniqueRuleSetToStyleSheet(keyframe);
                return keyframe.Selector;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in CSS string:");
                Console.Error.WriteLine(css);
                throw new Exception("Parse Error", e);
            }
        }

        public async Task Fontface(string css)
        {
            try
            {
                var fontface = ParseFontFace(css);
                await AddUniqueRuleSetToStyleSheet(fontface);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error in CSS string:");
                Console.Error.WriteLine(css);
                throw new Exception("Parse Error", e);
            }
        }

        private Keyframe ParseKeyframe(string css)
        {
            var keyframe = new Keyframe();
            IRule current = keyframe;
            string buffer = string.Empty;
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
                        break;
                    case '}':
                        break;
                    default:
                        buffer += ch;
                        break;
                }
            }
            keyframe.SetClassName();
            return keyframe;
        }

        private FontFace ParseFontFace(string css)
        {
            var fontface = new FontFace
            {
                Declarations = ParseDeclarations(css)
            };
            fontface.SetClassName();
            return fontface;
        }

        private PredefinedRuleSet ParsePredefinedRuleSet(string className, string css)
        {
            var predefinedRuleSet = new PredefinedRuleSet { Selector = className.Trim() };
            predefinedRuleSet.Declarations = ParseDeclarations(css);
            return predefinedRuleSet;
        }

        private List<Declaration> ParseDeclarations(string css)
        {
            var declarations = new List<Declaration>();
            var declarationsString = css.Trim().Split(';');
            foreach (var declarationString in declarationsString)
            {
                if (declarationString.IndexOf(':') != -1)
                {
                    var declaration = ParseDeclaration(declarationString.Trim());
                    if (declaration != null) declarations.Add(declaration);
                }
            }
            return declarations;
        }

        private RuleSet ParseRuleSet(string css)
        {
            RuleSet ruleSet = new RuleSet();
            IRule current = ruleSet;
            string buffer = string.Empty;
            foreach (char ch in css)
            {
                switch (ch)
                {
                    case ';':
                        var declaration = ParseDeclaration(buffer.Trim());
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
                            nestedClass = new PredefinedRuleSet();
                            nestedClass.Selector = buffer.Trim();
                        }
                        else
                        {
                            nestedClass = new MediaQuery();
                            nestedClass.Selector = buffer.Trim() + "{&";
                        }
                        ruleSet.NestedRules.Add(nestedClass);
                        buffer = string.Empty;
                        current = nestedClass;
                        break;
                    case '}':
                        break;
                    default:
                        buffer += ch;
                        break;
                }
            }
            ruleSet.SetClassName();
            return ruleSet;
        }

        private Declaration ParseDeclaration(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            try
            {
                var property = input.Substring(0, input.IndexOf(':'));
                var value = input.Substring(input.IndexOf(':') + 1);
                return new Declaration { Property = property, Value = value };
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Declaration: " + input);
                throw e;
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
    }
}
