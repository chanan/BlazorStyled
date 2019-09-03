using BlazorStyled.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BlazorStyled.Stylesheets
{
    internal class StyleSheet : IEnumerable<IRule>, IObservable<IStyleSheet>, IStyleSheet
    {
        private const string DEFAULT = "Default";
        private readonly List<IObserver<IStyleSheet>> _observers = new List<IObserver<IStyleSheet>>();
        private readonly IDictionary<string, IDictionary<string, IRule>> _classes = new Dictionary<string, IDictionary<string, IRule>>();
        private readonly Hash _hash = new Hash();

        public void ClearStyles(string id)
        {
            string key = id ?? DEFAULT;
            if (_classes.ContainsKey(key))
            {
                _classes[key].Clear();
                NotifyObservers();
            }
        }

        public void AddClass(IRule rule, string id)
        {
            IDictionary<string, IRule> classes = GetClassesForId(id);
            bool notify = false;
            if (rule.Selector.Contains("-"))
            {
                if (!classes.ContainsKey(rule.Hash))
                {
                    classes.Add(rule.Hash, rule);
                    notify = true;
                }
            }
            else
            {
                //merge html elements
                notify = true;
                if (TryGetHtmlElementClass(classes, rule.Selector, out PredefinedRuleSet existing))
                {
                    MergeHtmlElements(existing, rule);
                }
                else
                {
                    classes.Add(rule.Hash, rule);
                }
            }
            if (notify)
            {
                NotifyObservers();
            }
        }

        private void MergeHtmlElements(PredefinedRuleSet existing, IRule rule)
        {
            foreach (Declaration newDecelearion in rule.Declarations)
            {
                existing.MergeDeceleration(newDecelearion);
            }
        }

        public int Count => _classes.Count;

        public Theme Theme { get; set; } = new Theme();

        public string GetHashCodes()
        {
            List<string> list = (from classes in _classes.Values
                                 from cssClass in classes.Values
                                 select cssClass.Hash).ToList();
            return string.Join("", list);
        }

        //Return @import at the top of the list
        public IEnumerator<IRule> GetEnumerator()
        {
            List<IRule> list = new List<IRule>();
            List<IRule> imports = (from classes in _classes.Values
                                   from rule in classes.Values
                                   where rule.RuleType == RuleType.Import
                                   select rule).ToList();
            List<IRule> notImports = (from classes in _classes.Values
                                      from rule in classes.Values
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
            IEnumerable<IRule> q = from classes in _classes.Values
                                   from rule in classes.Values
                                   where rule.RuleType != RuleType.Import
                                   select rule;

            return q.ToList().Distinct(new RuleComparer()).AsEnumerable();
        }

        public IEnumerable<string> GetImportRules()
        {
            List<string> list = new List<string>();
            List<IRule> rules = (from classes in _classes.Values
                                 from rule in classes.Values
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

        public IDisposable Subscribe(IObserver<IStyleSheet> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Unsubscriber<IStyleSheet>(_observers, observer);
        }

        public IList<IRule> GetRules(string id, string selector)
        {
            List<IRule> ret = new List<IRule>();
            IDictionary<string, IRule> classes = GetClassesForId(id);
            ret.Add(classes[selector]);
            string classname = "." + selector;
            IEnumerable<IRule> q = from r in classes.Values
                                   where r.Selector.StartsWith(classname) && r.Selector != classname
                                   select r;
            ret.AddRange(q.ToList());
            return ret;
        }

        public void SetThemeValue(string name, string value)
        {
            if (Theme.Values.ContainsKey(name))
            {
                Theme.Values[name] = value;
            }
            else
            {
                Theme.Values.Add(name, value);
            }
            NotifyObservers();
        }

        public IEnumerable<KeyValuePair<string, string>> GetThemeValues()
        {
            return Theme.Values.ToList().AsEnumerable();
        }

        public int GetThemeHashCode()
        {
            int result = 0;
            foreach (KeyValuePair<string, string> kvp in GetThemeValues())
            {
                result += kvp.Key.GetStableHashCode();
                result += kvp.Value.GetStableHashCode();
            }
            return result;
        }

        private void NotifyObservers()
        {
            foreach (IObserver<IStyleSheet> observer in _observers)
            {
                observer.OnNext(this);
            }
        }

        private bool TryGetHtmlElementClass(IDictionary<string, IRule> classes, string selector, out PredefinedRuleSet existing)
        {
            IEnumerable<IRule> q = from r in classes.Values
                                   where r.RuleType == RuleType.PredefinedRuleSet && r.Selector == selector
                                   select r;
            existing = (PredefinedRuleSet)q.SingleOrDefault();
            return existing != null;
        }

        private IDictionary<string, IRule> GetClassesForId(string id)
        {
            string key = id ?? DEFAULT;
            if (!_classes.ContainsKey(key))
            {
                _classes.Add(key, new Dictionary<string, IRule>());
            }
            return _classes[key];
        }
    }

    internal class Unsubscriber<IStyleSheet> : IDisposable
    {
        private readonly List<IObserver<IStyleSheet>> _observers;
        private readonly IObserver<IStyleSheet> _observer;

        internal Unsubscriber(List<IObserver<IStyleSheet>> observers, IObserver<IStyleSheet> observer)
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
