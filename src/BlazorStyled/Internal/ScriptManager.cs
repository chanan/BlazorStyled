using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStyled.Internal
{
    internal class ScriptManager
    {
        private IJSRuntime JSRuntime { get; set; }
        private IConfig Config { get; set; }

        private bool _init = false;

        public ScriptManager(IJSRuntime jSRuntime, IConfig config)
        {
            JSRuntime = jSRuntime;
            Config = config;
        }

        private async Task Init()
        {
            if (!_init)
            {
                await JSRuntime.InvokeVoidAsync("eval", _script);
                _init = true;
            }
        }

        internal async Task UpdatedParsedClasses(string stylesheetId, string stylesheetName, int priority, IList<ParsedClass> parsedClasses)
        {
            await Init();
            string[] rules = parsedClasses.Select(c => c.ToString()).ToArray();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.insertClasses", stylesheetId, stylesheetName, priority, rules, Config.IsDevelopment, Config.IsDebug);
        }

        internal async Task SetThemeValue(string stylesheetId, string stylesheetName, int priority, string name, string value)
        {
            await Init();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.setThemeValue", stylesheetId, stylesheetName, priority, name, value, Config.IsDevelopment, Config.IsDebug);
        }

        internal async Task<IDictionary<string, string>> GetThemeValues(string stylesheetId)
        {
            await Init();
            return await JSRuntime.InvokeAsync<Dictionary<string, string>>("BlazorStyled.getThemeValues", stylesheetId);
        }

        internal async Task SetGlobalStyle(string stylesheetId, string name, string value)
        {
            await Init();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.setGlobalStyle", stylesheetId, name, value);
        }

        internal async Task<IDictionary<string, string>> GetGlobalStyles(string stylesheetId)
        {
            await Init();
            return await JSRuntime.InvokeAsync<Dictionary<string, string>>("BlazorStyled.getGlobalStyles", stylesheetId);
        }

        internal async Task<string> GetGlobalStyle(string stylesheetId, string name)
        {
            await Init();
            IDictionary<string, string> styles = await JSRuntime.InvokeAsync<Dictionary<string, string>>("BlazorStyled.getGlobalStyles", stylesheetId);
            return styles[name];
        }

        internal async Task ClearStyles(string stylesheetId, string stylesheetName)
        {
            await Init();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.clearStyles", stylesheetId, stylesheetName, Config.IsDebug);
        }

        //Script
        private readonly static string _script = "window.BlazorStyled ={insertClasses:function(n, t, i, r, u, f) { for (var e = 0; e < r.length; e++) { const o= r[e]; window.BlazorStyled.insertClass(n, t, i, o, u, f)} },insertClass:function(n, t, i, r, u, f) { const e= window.BlazorStyled.initLogger(f), s = window.BlazorStyled.getOrCreateSheet(n, t, i, e), o = window.BlazorStyled.parseTheme(n, r, e); if (o) if (u) window.BlazorStyled.writeRule(s, o, e); else try { o.indexOf(':-moz') !== -1 && 'MozBoxSizing'in document.body.style? window.BlazorStyled.insertRule(s.sheet, o, e):o.indexOf(':-moz') === -1 ? window.BlazorStyled.insertRule(s.sheet, o, e) : e.warn('Mozilla rule not inserted: ', o)} catch (h) { e.error('Failed to insert: ', o); e.error(h)} },updateRule:function(n, t, i, r, u, f, e, o) { const s= window.BlazorStyled.initLogger(o), h = window.BlazorStyled.getOrCreateSheet(n, t, i, s), c = window.BlazorStyled.parseTheme(n, f, s); if (e) window.BlazorStyled.updateWrittenRule(h, u, c, s); else try { window.BlazorStyled.updatedInsertedRule(h.sheet, u, c, s)} catch (l) { s.error('Failed to update: ', f); s.error(l)} },clearStyles:function(n, t, i) { const u= window.BlazorStyled.initLogger(i), r = document.getElementById(n); r && (document.head.removeChild(r), u.log('Cleared stylesheet: ', t))},setThemeValue:function(n, t, i, r, u, f, e) { const o= window.BlazorStyled.initLogger(e); try { const h= window.BlazorStyled.getOrCreateTheme(n), c = h.values[r]; h.values[r] = u;for(var s in h.rules) { const u= h.rules[s]; if (u.indexOf(r) !== -1) if (c) { const h= u.substring(0, u.indexOf('{')), s = window.BlazorStyled.parseTheme(n, u.replace('[' + r + ']', c), o); s && window.BlazorStyled.updateRule(n, t, i, h, s, u, f, e)} else window.BlazorStyled.insertClass(n, t, i, u, f, e)} } catch (h) { o.error('Failed to update: ', rule); o.error(h)} },getThemeValues:function(n) { const t= window.BlazorStyled.getOrCreateTheme(n); return t.values},setGlobalStyle:function(n, t, i) { const r= window.BlazorStyled.getOrCreateTheme(n); r.globalStyles[t] = i},getGlobalStyles:function(n) { const t= window.BlazorStyled.getOrCreateTheme(n); return t.globalStyles},parseTheme:function(n, t, i) { if (t.indexOf('[') === -1) return t; const r= window.BlazorStyled.getOrCreateTheme(n); r.rules.find(n => n === t) || r.rules.push(t); const u= t.substring(t.indexOf('[') + 1, t.indexOf(']')), f = r.values[u]; if (f === undefined) return undefined; const e= t.replace('[' + u + ']', f); return window.BlazorStyled.parseTheme(n, e, i)},getOrCreateTheme:function(n) { return window.BlazorStyled.themes[n] === undefined && (window.BlazorStyled.themes[n] ={ values: { },rules:[],globalStyles: { } }),window.BlazorStyled.themes[n]},initLogger:function(n) { var t, i; if (this.debug ={ },n)for (t in console) typeof console[t]== 'function' && (this.debug[t] = console[t].bind(window.console));else for (i in console) typeof console[i]== 'function' && (this.debug[i] = function(){ }); return this.debug},getOrCreateSheet:function(n, t, i, r) { const e= 'data-blazorstyled-priority', o = document.getElementById(n); if (o) return o; const f= document.createElement('style'), s = document.createAttribute('id'); s.value = n; f.setAttributeNode(s); const h= document.createAttribute('data-blazorstyled-name'); h.value = t; f.setAttributeNode(h); const c= document.createAttribute(e); c.value = i; f.setAttributeNode(c); const u= document.head; if (u.hasChildNodes()) { let n = !1; for (let t = 0; t < u.children.length; t++) { const r= u.children[t]; if (r.hasAttribute(e)) { const o= r.getAttribute(e), s = parseInt(o, 10); i >= s && !n && (n = !0, t !== u.children.length - 1 ? u.insertBefore(f, u.children[t + 1]) : u.appendChild(f))} } n || u.insertBefore(f, u.firstChild)} else u.appendChild(f); return r.log('Inserted stylesheet: ', f),f},writeRule:function(n, t, i) { n.innerText? n.innerText.indexOf(t) === -1 && (n.innerText = t.startsWith('@import') ? t + n.innerText : n.innerText + t, i.log('Written: ', t)):(n.innerText = t, i.log('Written: ', t))},insertRule:function(n, t, i) { const r= t.startsWith('@import') ? 0 : n.cssRules.length; n.insertRule(t, r); i.log('Inserted at ' + r + ': ', t)},updateWrittenRule:function(n, t, i, r) { n.innerText || (n.innerText = i); n.innerText = n.innerText.replace(t, i); r.log('Updated old rule: ' + t + ' to new rule: ' + i)},updatedInsertedRule:function(n, t, i, r) { const e= window.BlazorStyled.getOrCreateSheet('temp', 'temp', window.BlazorStyled.initLogger(!1)); e.sheet.insertRule(t); const o= e.sheet.cssRules[0].cssText; document.head.removeChild(e); let u = -1; for (var f = 0; f < n.cssRules.length; f++) n.cssRules[f].cssText === o && (u = f); u !== -1 && (n.deleteRule(u), n.insertRule(i, u), r.log('Updated old rule at ' + u + ': ' + t + ' to new rule: ' + i))},themes:{}};";
    }
}
