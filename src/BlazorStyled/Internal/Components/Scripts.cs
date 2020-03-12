using BlazorStyled.Stylesheets;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStyled.Internal.Components
{
    internal class Scripts : ComponentBase, IObserver<RuleContext>, IDisposable
    {
        private IDisposable _unsubscriber;

        //Injection
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IStyleSheet StyleSheet { get; set; }
        [Inject] private IConfig Config { get; set; }

        //Commands
        private async Task InsertRule(RuleContext ruleContext)
        {
            string rule = ApplyTheme(ruleContext.Stylesheet.Theme, ruleContext.Rule.ToString());
            await JSRuntime.InvokeVoidAsync("BlazorStyled.insertRule", ruleContext.Stylesheet.Hash, ruleContext.Stylesheet.Name, rule, Config.IsDevelopment, Config.IsDebug);
        }

        private async Task ThemeValueUpdated(RuleContext ruleContext)
        {
            string key = "[" + ruleContext.ThemeEntry.Key + "]";
            //Elements
            List<KeyValuePair<string, IRule>> elementRules = (from elementRuleList in ruleContext.Stylesheet.Elements
                                                              from rule in elementRuleList.Value
                                                              where rule.Value.ToString().IndexOf(key) != -1
                                                              select new KeyValuePair<string, IRule>(elementRuleList.Key, rule.Value)).ToList();

            foreach (KeyValuePair<string, IRule> kvp in elementRules)
            {
                string oldRule = ApplyTheme(ruleContext.Stylesheet.Theme, kvp.Value.ToString(), ruleContext.ThemeEntry.Key, ruleContext.OldThemeValue);
                string rule = ApplyTheme(ruleContext.Stylesheet.Theme, kvp.Value.ToString());
                await JSRuntime.InvokeVoidAsync("BlazorStyled.updateRule", ruleContext.Stylesheet.Hash, ruleContext.Stylesheet.Name, kvp.Key, oldRule, rule, Config.IsDevelopment, Config.IsDebug);
            }

            //Classes
            List<KeyValuePair<string, IRule>> classRules = (from rule in ruleContext.Stylesheet.Classes
                                                            where rule.Value.ToString().IndexOf(key) != -1
                                                            select new KeyValuePair<string, IRule>(rule.Key, rule.Value)).ToList();

            foreach (KeyValuePair<string, IRule> kvp in classRules)
            {
                string oldRule = ApplyTheme(ruleContext.Stylesheet.Theme, kvp.Value.ToString(), ruleContext.ThemeEntry.Key, ruleContext.OldThemeValue);
                string rule = ApplyTheme(ruleContext.Stylesheet.Theme, kvp.Value.ToString());
                await JSRuntime.InvokeVoidAsync("BlazorStyled.updateRule", ruleContext.Stylesheet.Hash, ruleContext.Stylesheet.Name, kvp.Key, oldRule, rule, Config.IsDevelopment, Config.IsDebug);
            }
        }

        private async Task ClearStyles(RuleContext ruleContext)
        {
            await JSRuntime.InvokeVoidAsync("BlazorStyled.clearStyles", ruleContext.Stylesheet.Hash, ruleContext.Stylesheet.Name, Config.IsDebug);
        }

        //Component
        protected override void OnInitialized()
        {
            _unsubscriber = StyleSheet.Subscribe(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("eval", _script);
                await StyleSheet.BecomeScriptTag();
                StyleSheet.UnbecomingScriptTag();
            }
        }

        //IObserver<RuleContext>
        public void OnCompleted()
        {
            _unsubscriber.Dispose();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public async void OnNext(RuleContext ruleContext)
        {
            await HandleRuleContext(ruleContext);
        }

        private async Task HandleRuleContext(RuleContext ruleContext)
        {
            switch (ruleContext.Event)
            {
                case RuleContextEvent.AddClass:
                    await InsertRule(ruleContext);
                    break;
                case RuleContextEvent.ClearStyles:
                    await ClearStyles(ruleContext);
                    break;
                case RuleContextEvent.ThemeValueChanged:
                    await ThemeValueUpdated(ruleContext);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private string ApplyTheme(Theme theme, string content)
        {
            foreach (KeyValuePair<string, string> kvp in theme.GetTheme())
            {
                if (content.Contains("[" + kvp.Key + "]"))
                {
                    content = ApplyThemeValue(content, kvp.Key, kvp.Value);
                }
            }
            return content;
        }

        private string ApplyTheme(Theme theme, string content, string overideKey, string overrideValue)
        {
            foreach (KeyValuePair<string, string> kvp in theme.GetTheme())
            {
                string value = kvp.Key == overideKey ? overrideValue : kvp.Value;
                if (content.Contains("[" + kvp.Key + "]"))
                {
                    content = ApplyThemeValue(content, kvp.Key, value);
                }
            }
            return content;
        }

        private string ApplyThemeValue(string content, string key, string value)
        {
            return content.Replace("[" + key + "]", value);
        }

        //IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_unsubscriber != null)
                {
                    _unsubscriber.Dispose();
                    StyleSheet.UnbecomingScriptTag();
                    StyleSheet.UnbecomeScriptTag();
                }
            }
        }

        //Script
        private readonly string _script = "function initDebug(n){var t,i;if(this.debug={},n)for(t in console)typeof console[t]=='function'&&(this.debug[t]=console[t].bind(window.console));else for(i in console)typeof console[i]=='function'&&(this.debug[i]=function(){});return this.debug}function getOrCreateSheet(n,t,i){const f=document.getElementById(n);if(f)return f;const r=document.createElement('style'),e=document.createAttribute('id');e.value=n;r.setAttributeNode(e);const o=document.createAttribute('data-blazorstyled-stylesheet-name');o.value=t;r.setAttributeNode(o);const u=document.head;return u.firstChild?t==='Default'?u.appendChild(r):u.insertBefore(r,u.firstChild):u.appendChild(r),i.log('Inserted stylesheet: ',r),r}function writeRule(n,t,i){n.innerText||(n.innerText=t);n.innerText=t.startsWith('@import')?t+n.innerText:n.innerText+t;i.log('Written: ',t)}function insertRule(n,t,i){const r=t.startsWith('@import')?0:n.cssRules.length;n.insertRule(t,r);i.log('Inserted at '+r+': ',t)}function updateWrittenRule(n,t,i,r){n.innerText||(n.innerText=i);n.innerText=n.innerText.replace(t,i);r.log('Updated old rule: '+t+' to new rule: '+i)}function updatedInsertedRule(n,t,i,r){const e=getOrCreateSheet('temp','temp',initDebug(!1));e.sheet.insertRule(t);const o=e.sheet.cssRules[0].cssText;document.head.removeChild(e);let u=-1;for(var f=0;f<n.cssRules.length;f++)n.cssRules[f].cssText===o&&(u=f);u!==-1&&(n.deleteRule(u),n.insertRule(i,u),r.log('Updated old rule at '+u+': '+t+' to new rule: '+i))}window.BlazorStyled={insertRule:function(n,t,i,r,u){const f=initDebug(u),e=getOrCreateSheet(n,t,f);if(r)writeRule(e,i,f);else try{i.indexOf(':-moz')!==-1&&'MozBoxSizing'in document.body.style?insertRule(e.sheet,i,f):i.indexOf(':-moz')===-1?insertRule(e.sheet,i,f):f.warn('Mozilla rule not inserted: ',i)}catch(o){f.error('Failed to insert: ',i);f.error(o)}},updateRule:function(n,t,i,r,u,f,e){const o=initDebug(e),s=getOrCreateSheet(n,t,o);if(f)updateWrittenRule(s,r,u,o);else try{updatedInsertedRule(s.sheet,r,u,o)}catch(h){o.error('Failed to update: ',u);o.error(h)}},clearStyles:function(n,t,i){const u=initDebug(i),r=document.getElementById(n);r&&(document.head.removeChild(r),u.log('Cleared stylesheet: ',t))}};";
    }
}