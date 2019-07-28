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
                foreach (IObserver<IStyleSheet> observer in _observers)
                {
                    observer.OnNext(this);
                }
            }
        }

        public void AddClass(IRule rule, string id)
        {
            IDictionary<string, IRule> classes = GetClassesForId(id);
            if (rule.Selector.StartsWith("."))
            {
                if (!classes.ContainsKey(rule.Selector))
                {
                    classes.Add(rule.Selector, rule);
                }
            }
            else
            {
                if (!classes.ContainsKey(rule.Selector))
                {
                    classes.Add(rule.Selector, rule);
                }
                else
                {
                    //For non class rules such as html elements check to see if they are the same before adding
                    IRule existingRule = classes[rule.Selector];
                    if (_hash.GetHashCode(existingRule) != _hash.GetHashCode(rule))
                    {
                        MergeClasses(existingRule, rule);
                    }

                }
            }
            foreach (IObserver<StyleSheet> observer in _observers)
            {
                observer.OnNext(this);
            }
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

        private void MergeClasses(IRule existingRule, IRule rule)
        {
            //TODO: Only merge new classes/rules
            if (existingRule.RuleType != RuleType.Keyframe)
            {
                existingRule.Declarations.AddRange(rule.Declarations);
            }
            if (existingRule.RuleType != RuleType.FontFace && existingRule.RuleType != RuleType.PredefinedRuleSet)
            {
                existingRule.NestedRules.AddRange(rule.NestedRules);
            }
        }

        public int Count => _classes.Count;

        public string GetHashCodes()
        {
            List<string> list = (from classes in _classes.Values
                                 from cssClass in classes.Values
                                 select _hash.GetHashCode(cssClass)).ToList();
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
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDisposable Subscribe(IObserver<IStyleSheet> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Unsubscriber<IStyleSheet>(_observers, observer);
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
}
