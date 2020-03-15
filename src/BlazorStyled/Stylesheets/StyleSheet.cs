using BlazorStyled.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorStyled.Stylesheets
{
    internal class StyleSheet : IEnumerable<IRule>, IStyleSheet, IObservable<RuleContext>
    {
        private const string DEFAULT = "Default";
        private readonly List<IObserver<RuleContext>> _ruleObservers = new List<IObserver<RuleContext>>();
        private readonly IDictionary<string, StyleSheetMetadata> _stylesheets = new Dictionary<string, StyleSheetMetadata>
        {
            { DEFAULT, new StyleSheetMetadata { Name = DEFAULT, Hash = DEFAULT.GetRandomHashCodeString() } }
        };
        private readonly Queue<RuleContext> _storage = new Queue<RuleContext>();
        private SemaphoreSlim _scriptRenderedSemaphore = new SemaphoreSlim(1, 1);
        private SemaphoreSlim _becomingScriptTagSemaphore = new SemaphoreSlim(1, 1);

        public bool ScriptRendered { get; private set; } = false;
        public bool ScriptRendering { get; private set; } = false;
        public Queue<RuleContext> Storage { get; set; } = new Queue<RuleContext>();

        public async ValueTask<bool> BecomingScriptTag()
        {
            if (ScriptRendered || ScriptRendering)
            {
                return false;
            }

            bool hasBecome = false;
            await _becomingScriptTagSemaphore.WaitAsync();
            try
            {
                if (!ScriptRendered && !ScriptRendering)
                {
                    ScriptRendering = true;
                    hasBecome = true; ;
                }
            }
            finally
            {
                _becomingScriptTagSemaphore.Release();

            }
            return hasBecome;
        }

        public void UnbecomingScriptTag()
        {
            ScriptRendering = false;
        }

        public async ValueTask<bool> BecomeScriptTag()
        {
            if (ScriptRendered)
            {
                return false;
            }

            bool hasBecome = false;
            await _scriptRenderedSemaphore.WaitAsync();
            try
            {
                if (!ScriptRendered)
                {
                    ScriptRendered = true;
                    hasBecome = true; ;
                    NotifyObserversOfSaveRules();
                }
            }
            finally
            {
                _scriptRenderedSemaphore.Release();

            }
            return hasBecome;
        }

        public void UnbecomeScriptTag()
        {
            ScriptRendered = false;
        }

        public void ClearStyles(string id)
        {
            string key = id ?? DEFAULT;
            if (_stylesheets.ContainsKey(key))
            {
                _stylesheets[key].Classes.Clear();
                _stylesheets[key].Elements.Clear();
                NotifyRuleObservers(new RuleContext { Event = RuleContextEvent.ClearStyles, Stylesheet = _stylesheets[key] });
            }
        }

        public void AddClass(IRule rule, string id)
        {
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            bool notify = false;
            if (rule.Selector.Contains("-"))
            {
                IDictionary<string, IRule> classes = styleSheetMetadata.Classes;
                if (!classes.ContainsKey(rule.Hash))
                {
                    classes.Add(rule.Hash, rule);
                    notify = true;
                }
            }
            else
            {
                IDictionary<string, IDictionary<string, IRule>> elements = styleSheetMetadata.Elements;
                if (IsExistsHtmlElementClass(elements, rule))
                {
                    notify = false;
                }
                else
                {
                    AddHtmlElementClass(elements, rule);
                    notify = true;
                }
            }
            if (notify)
            {
                NotifyRuleObservers(new RuleContext { Event = RuleContextEvent.AddClass, Stylesheet = styleSheetMetadata, Rule = rule });
            }
        }

        public int Count => _stylesheets.Sum(sheet => sheet.Value.Classes.Count);

        public string GetHashCodes()
        {
            List<string> list = (from sheet in _stylesheets.Values
                                 from cssClass in sheet.Classes.Values
                                 select cssClass.Hash).ToList();
            return string.Join("", list);
        }

        //Return @import at the top of the list
        public IEnumerator<IRule> GetEnumerator()
        {
            List<IRule> list = new List<IRule>();
            List<IRule> imports = (from sheet in _stylesheets.Values
                                   from rule in sheet.Classes.Values
                                   where rule.RuleType == RuleType.Import
                                   select rule).ToList();
            List<IRule> notImports = (from sheet in _stylesheets.Values
                                      from rule in sheet.Classes.Values
                                      where rule.RuleType != RuleType.Import
                                      select rule).ToList();
            list.AddRange(imports);
            list.AddRange(notImports);
            return list.Distinct(new RuleComparer()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<IRule> GetRulesWithoutImport()
        {
            IEnumerable<IRule> q = from sheet in _stylesheets.Values
                                   from rule in sheet.Classes.Values
                                   where rule.RuleType != RuleType.Import
                                   select rule;

            return q.ToList().Distinct(new RuleComparer()).AsEnumerable();
        }

        public IEnumerable<string> GetImportRules()
        {
            List<string> list = new List<string>();
            List<IRule> rules = (from sheet in _stylesheets.Values
                                 from rule in sheet.Classes.Values
                                 where rule.RuleType == RuleType.Import
                                 select rule).ToList();

            foreach (IRule rule in rules)
            {
                ImportUri import = (ImportUri)rule;
                if (!list.Contains(import.Declarations.First().Value))
                {
                    list.Add(import.Declarations.First().Value);
                }
            }
            return list.AsEnumerable();
        }

        public IList<IRule> GetRules(string id, string selector)
        {
            List<IRule> ret = new List<IRule>();
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            IDictionary<string, IRule> classes = styleSheetMetadata.Classes;
            ret.Add(classes[selector]);
            string classname = "." + selector;
            IEnumerable<IRule> q = from r in classes.Values
                                   where r.Selector.StartsWith(classname) && r.Selector != classname
                                   select r;
            ret.AddRange(q.ToList());
            return ret;
        }

        public void SetThemeValue(string id, string name, string value)
        {
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            string oldValue = styleSheetMetadata.Theme.AddOrUpdate(name, value);
            if (oldValue != null)
            {
                NotifyRuleObservers(new RuleContext
                {
                    Event = RuleContextEvent.ThemeValueChanged,
                    Stylesheet = styleSheetMetadata,
                    ThemeEntry = new KeyValuePair<string, string>(name, value),
                    OldThemeValue = oldValue
                });
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetThemeValues(string id)
        {
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            return styleSheetMetadata.Theme.GetTheme().ToList().AsEnumerable();
        }

        private void NotifyRuleObservers(RuleContext ruleContext)
        {
            if (!ScriptRendered)
            {
                _storage.Enqueue(ruleContext);
            }
            if (ScriptRendered)
            {
                NotifyObserversOfSaveRules();

                foreach (IObserver<RuleContext> observer in _ruleObservers)
                {
                    observer.OnNext(ruleContext);
                }
            }
        }

        private void NotifyObserversOfSaveRules()
        {
            foreach (RuleContext rule in _storage)
            {
                foreach (IObserver<RuleContext> observer in _ruleObservers)
                {
                    observer.OnNext(rule);
                }
            }
            _storage.Clear();
        }

        private bool IsExistsHtmlElementClass(IDictionary<string, IDictionary<string, IRule>> elements, IRule rule)
        {
            IEnumerable<IRule> q = from rules in elements
                                   from r in rules.Value
                                   where rules.Key == rule.Selector &&
                                   r.Value.RuleType == RuleType.PredefinedRuleSet &&
                                   r.Key == rule.ToString().GetStableHashCodeString()
                                   select r.Value;

            return q.Count() != 0;
        }

        private void AddHtmlElementClass(IDictionary<string, IDictionary<string, IRule>> elements, IRule rule)
        {
            if (!elements.ContainsKey(rule.Selector))
            {
                elements.Add(rule.Selector, new Dictionary<string, IRule>());
            }
            IDictionary<string, IRule> elementRules = elements[rule.Selector];
            string key = rule.ToString().GetStableHashCodeString();
            if (!elementRules.ContainsKey(key))
            {
                elementRules.Add(key, rule);
            }
        }

        private StyleSheetMetadata GetStyleSheetForId(string id)
        {
            string key = id ?? DEFAULT;
            if (!_stylesheets.ContainsKey(key))
            {
                _stylesheets.Add(key, new StyleSheetMetadata { Name = key, Hash = key.GetStableHashCodeString() });
            }
            return _stylesheets[key];
        }

        public IDisposable Subscribe(IObserver<RuleContext> observer)
        {
            if (!_ruleObservers.Contains(observer))
            {
                _ruleObservers.Add(observer);
            }
            return new RuleUnsubscriber<RuleContext>(_ruleObservers, observer);
        }

        public string GlobalStyle(string id, string globalClassName)
        {
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            return styleSheetMetadata.GlobalStyles[globalClassName];
        }

        public void AddOrUpdateGlobalStyle(string id, string globalClassName, string className)
        {
            StyleSheetMetadata styleSheetMetadata = GetStyleSheetForId(id);
            styleSheetMetadata.GlobalStyles[globalClassName] = className;
        }
    }

    internal class RuleUnsubscriber<RuleContext> : IDisposable
    {
        private readonly List<IObserver<RuleContext>> _observers;
        private readonly IObserver<RuleContext> _observer;

        internal RuleUnsubscriber(List<IObserver<RuleContext>> observers, IObserver<RuleContext> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }

    internal class RuleComparer : IEqualityComparer<IRule>
    {
        public bool Equals(IRule x, IRule y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y == null)
            {
                return false;
            }

            return x.Hash == y.Hash;
        }

        public int GetHashCode(IRule obj)
        {
            return obj.Hash.GetHashCode();
        }
    }
}
