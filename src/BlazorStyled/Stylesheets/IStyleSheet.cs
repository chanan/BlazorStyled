using System;
using System.Collections.Generic;

namespace BlazorStyled.Stylesheets
{
    public interface IStyleSheet
    {
        int Count { get; }
        string Id { get; }

        void AddClass(IRule rule);
        void ClearStyles();
        IEnumerator<IRule> GetEnumerator();
        string GetHashCodes();
        IDisposable Subscribe(IObserver<IStyleSheet> observer);
    }
}