using BlazorStyled.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BlazorStyled.Stylesheets
{
    public class StyleSheet : IEnumerable<IRule>, IObservable<StyleSheet>
    {
        private readonly List<IObserver<StyleSheet>> _observers = new List<IObserver<StyleSheet>>();
        private IDictionary<string, IRule> _classes = new Dictionary<string, IRule>();
        private readonly Hash _hash = new Hash();

        public void ClearStyles()
        {
            _classes = new Dictionary<string, IRule>();
            foreach (IObserver<StyleSheet> observer in _observers)
            {
                observer.OnNext(this);
            }
        }

        public void AddClass(IRule rule)
        {
            if (rule.Selector.StartsWith("."))
            {
                if (!_classes.ContainsKey(rule.Selector))
                {
                    _classes.Add(rule.Selector, rule);
                }
            }
            else
            {
                if (!_classes.ContainsKey(rule.Selector))
                {
                    _classes.Add(rule.Selector, rule);
                }
                else
                {
                    //For non class rules such as html elements check to see if they are the same before adding
                    IRule existingRule = _classes[rule.Selector];
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
            List<string> list = (from cssClass in _classes.Values
                                 select _hash.GetHashCode(cssClass)).ToList();
            return string.Join("", list);
        }

        public IEnumerator<IRule> GetEnumerator()
        {
            return _classes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _classes.Values.GetEnumerator();
        }

        public IDisposable Subscribe(IObserver<StyleSheet> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Unsubscriber<StyleSheet>(_observers, observer);
        }
    }

    internal class Unsubscriber<StyleSheet> : IDisposable
    {
        private readonly List<IObserver<StyleSheet>> _observers;
        private readonly IObserver<StyleSheet> _observer;

        internal Unsubscriber(List<IObserver<StyleSheet>> observers, IObserver<StyleSheet> observer)
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
