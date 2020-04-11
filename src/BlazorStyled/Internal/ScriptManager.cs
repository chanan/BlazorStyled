using Microsoft.JSInterop;
using System;
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

        /**
         *  dev mode (This is not implemented)
         *  dynamic classes, font faces (use hash), keyframes - don't add if exist
         *  html elements, static classes - merge with existing classes
         *  media queries - add/find external class then do the above
         * */
        internal async Task UpdatedParsedClasses(string stylesheetId, string stylesheetName, IList<ParsedClass> parsedClasses)
        {
            await Init();
            string[] rules = parsedClasses.Select(c => c.ToString()).ToArray();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.insertClasses", stylesheetId, stylesheetName, rules, Config.IsDevelopment, Config.IsDebug);
        }

        internal async Task SetThemeValue(string stylesheetId, string stylesheetName, string name, string value)
        {
            await Init();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.setThemeValue", stylesheetId, stylesheetName, name, value, Config.IsDevelopment, Config.IsDebug);
        }

        internal async Task<IDictionary<string, string>> GetThemeValues(string stylesheetId)
        {
            await Init();
            return await JSRuntime.InvokeAsync<Dictionary<string, string>> ("BlazorStyled.getThemeValues", stylesheetId);
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

        //Script
        private readonly string _script = @"window.BlazorStyled = {
    insertClasses: function (stylesheetId, stylesheetName, rules, development, debug) {
        for (var i = 0; i < rules.length; i++) {
            const rule = rules[i];
            window.BlazorStyled.insertRule(stylesheetId, stylesheetName, rule, development, debug);
        }
    },
    insertRule: function (stylesheetId, stylesheetName, rule, development, debug) {
        const logger = initLogger(debug);
        const sheet = getOrCreateSheet(stylesheetId, stylesheetName, logger);
        const updatedRule = parseTheme(stylesheetId, rule, logger);
        if(updatedRule)
        {
            if (development) {
                writeRule(sheet, updatedRule, logger);
            } else {
                try {
                    if (updatedRule.indexOf(':-moz') !== -1 && 'MozBoxSizing' in document.body.style) {
                        insertRule(sheet.sheet, updatedRule, logger);
                    }
                    else if (updatedRule.indexOf(':-moz') === -1) {
                        insertRule(sheet.sheet, updatedRule, logger);
                    } else {
                        logger.warn('Mozilla rule not inserted: ', updatedRule);
                    }
                } catch (err) {
                    logger.error('Failed to insert: ', updatedRule);
                    logger.error(err);
                }
            }
        }
    },
    updateRule: function (stylesheetId, stylesheetName, selector, oldRule, rule, development, debug) {
        const logger = initLogger(debug);
        const sheet = getOrCreateSheet(stylesheetId, stylesheetName, logger);
        if (development) {
            updateWrittenRule(sheet, oldRule, rule, logger);
        } else {
            try {
                updatedInsertedRule(sheet.sheet, oldRule, rule, logger);
            } catch (err) {
                logger.error('Failed to update: ', rule);
                logger.error(err);
            }
        }
    },
    clearStyles: function (stylesheetId, stylesheetName, debug) {
        const logger = initLogger(debug);
        const sheet = document.getElementById(stylesheetId);
        if (sheet) {
            document.head.removeChild(sheet);
            logger.log('Cleared stylesheet: ', stylesheetName);
        }
    },
    setThemeValue: function (stylesheetId, stylesheetName, name, value, development, debug) {
        const theme = getOrCreateTheme(stylesheetId);
        theme.values[name] = value;
        for(var rule in theme.rules) {
            if(rule.indexOf(name) !== -1) {
                window.BlazorStyled.insertRule(stylesheetId, stylesheetName, rule, development, debug);
            }    
        }
    },
    getThemeValues: function (stylesheetId) {
        const theme = getOrCreateTheme(stylesheetId);
        return theme.values;
    },
    setGlobalStyle: function (stylesheetId, name, value) {
        const theme = getOrCreateTheme(stylesheetId);
        theme.globalStyles[name] = value;
    },
    getGlobalStyles: function (stylesheetId) {
        const theme = getOrCreateTheme(stylesheetId);
        return theme.globalStyles;
    },
    themes: {}
}

function parseTheme(stylesheetId, rule, logger) {
    if(rule.indexOf('[') === -1) {
        return rule;
    }
    const theme = getOrCreateTheme(stylesheetId);
    if(!theme.rules.find(r => r === rule)) {
        theme.rules.push(rule);
    }
    const themeValueName = rule.substring(rule.indexOf('[') + 1, rule.indexOf(']'));
    const themeValue = theme.values[themeValueName];
    if(themeValue === undefined) {
        return undefined;
    }
    const updated = rule.replace('[' + themeValueName + ']', themeValue);
    return parseTheme(stylesheetId, updated, logger);
}

function getOrCreateTheme(stylesheetId) {
    if(window.BlazorStyled.themes[stylesheetId] === undefined) {
        window.BlazorStyled.themes[stylesheetId] = {
            values: {},
            rules: [],
            globalStyles: {}
        }
    }
    return window.BlazorStyled.themes[stylesheetId];
}

function initLogger(debug) {
    this.debug = {};
    if (debug) {
        for (var m in console) {
            if (typeof console[m] === 'function') {
                this.debug[m] = console[m].bind(window.console);
            }
        }
    } else {
        for (var m2 in console) {
            if (typeof console[m2] === 'function') {
                this.debug[m2] = function () { };
            }
        }
    }
    return this.debug
}

function getOrCreateSheet(stylesheetId, stylesheetName, logger) {
    const sheet = document.getElementById(stylesheetId);
    if (sheet) return sheet;
    const styleEl = document.createElement('style');
    const id = document.createAttribute('id');
    id.value = stylesheetId;
    styleEl.setAttributeNode(id);
    const dataName = document.createAttribute('data-blazorstyled-stylesheet-name');
    dataName.value = stylesheetName;
    styleEl.setAttributeNode(dataName);
    const head = document.head;
    if (head.firstChild) {
        if (stylesheetName === 'Default') {
            head.appendChild(styleEl);
        } else {
            head.insertBefore(styleEl, head.firstChild);
        }
    } else {
        head.appendChild(styleEl);
    }
    logger.log('Inserted stylesheet: ', styleEl);
    return styleEl;
}

function writeRule(sheet, rule, logger)
{
    if (!sheet.innerText)
    {
        sheet.innerText = rule;
        logger.log('Written: ', rule);
    }
    else
    {
        if(sheet.innerText.indexOf(rule) == -1) 
        {
            sheet.innerText = rule.startsWith('@import') ? rule + sheet.innerText : sheet.innerText + rule;
            logger.log('Written: ', rule);
        }
    }
}
function insertRule(sheet, rule, logger)
{
    const index = rule.startsWith('@import') ? 0 : sheet.cssRules.length;
    sheet.insertRule(rule, index);
    logger.log('Inserted at ' + index + ': ', rule);
}

function updateWrittenRule(sheet, oldRule, rule, logger)
{
    if (!sheet.innerText)
    {
        sheet.innerText = rule;
    }
    sheet.innerText = sheet.innerText.replace(oldRule, rule);
    logger.log('Updated old rule: ' + oldRule + ' to new rule: ' + rule);
}

function updatedInsertedRule(sheet, oldRule, rule, logger)
{
    const temp = getOrCreateSheet('temp', 'temp', initLogger(false));
    temp.sheet.insertRule(oldRule);
    const oldCssText = temp.sheet.cssRules[0].cssText;
    document.head.removeChild(temp);
    let index = -1;
    for (var i = 0; i < sheet.cssRules.length; i++)
    {
        if (sheet.cssRules[i].cssText === oldCssText)
        {
            index = i;
        }
    }
    if (index !== -1)
    {
        sheet.deleteRule(index);
        sheet.insertRule(rule, index);
        logger.log('Updated old rule at ' + index + ': ' + oldRule + ' to new rule: ' + rule);
    }
}";
    }
}
