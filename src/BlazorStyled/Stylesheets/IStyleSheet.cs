using System;
using System.Collections.Generic;

namespace BlazorStyled.Stylesheets
{
    public interface IStyleSheet
    {
        int Count { get; }
        void AddClass(IRule rule, string id);
        void ClearStyles(string id);
        IEnumerator<IRule> GetEnumerator();
        string GetHashCodes();
        IEnumerable<string> GetImportRules();
        IEnumerable<IRule> GetRulesWithoutImport();
        IDisposable Subscribe(IObserver<IStyleSheet> observer);
        Theme Theme { get; set; }
    }
}