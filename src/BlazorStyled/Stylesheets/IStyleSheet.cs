using System;
using System.Collections.Generic;

namespace BlazorStyled.Stylesheets
{
    public interface IStyleSheet
    {
        int Count { get; }
        void AddClass(IRule rule, string id = null);
        void ClearStyles(string id = null);
        IEnumerator<IRule> GetEnumerator();
        string GetHashCodes();
        IDisposable Subscribe(IObserver<IStyleSheet> observer);
    }
}