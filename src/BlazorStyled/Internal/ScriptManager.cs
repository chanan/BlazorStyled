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
        private IDictionary<string, IDictionary<string, string>> _globalStyles = new Dictionary<string, IDictionary<string, string>>();
        private bool _init = false;

        public ScriptManager(IJSRuntime jSRuntime, IConfig config)
        {
            JSRuntime = jSRuntime;
            Config = config;
        }

        private async Task Init()
        {
            try
            {
                if (!_init)
                {
                    await JSRuntime.InvokeVoidAsync("eval", _script);
                    _init = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
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

        internal void SetGlobalStyle(string stylesheetId, string name, string value)
        {
            if (!_globalStyles.ContainsKey(stylesheetId))
            {
                _globalStyles.Add(stylesheetId, new Dictionary<string, string>());
            }
            if (_globalStyles.ContainsKey(name))
            {
                _globalStyles[stylesheetId][name] = value;
            }
            else
            {
                _globalStyles[stylesheetId].Add(name, value);
            }
        }

        internal IDictionary<string, string> GetGlobalStyles(string stylesheetId)
        {
            if (_globalStyles.ContainsKey(stylesheetId))
            {
                return _globalStyles[stylesheetId];
            }
            return null;
        }

        internal string GetGlobalStyle(string stylesheetId, string name)
        {
            if (_globalStyles.ContainsKey(stylesheetId))
            {
                return _globalStyles[stylesheetId][name];
            }
            return null;
        }

        internal async Task ClearStyles(string stylesheetId, string stylesheetName)
        {
            await Init();
            await JSRuntime.InvokeVoidAsync("BlazorStyled.clearStyles", stylesheetId, stylesheetName, Config.IsDebug);
        }

        //Script
        private static readonly string _script = @"window.BlazorStyled = {
  insertClasses: function (stylesheetId, stylesheetName, priority, rules, development, debug) {
    for (var i = 0; i < rules.length; i++) {
      const rule = rules[i];
      window.BlazorStyled.insertClass(stylesheetId, stylesheetName, priority, rule, development, debug);
    }
  },
  insertClass: function (stylesheetId, stylesheetName, priority, rule, development, debug) {
    const logger = window.BlazorStyled.initLogger(debug);
    const sheet = window.BlazorStyled.getOrCreateSheet(stylesheetId, stylesheetName, priority, logger);
    const updatedRule = window.BlazorStyled.parseTheme(stylesheetId, rule, logger);
    if (updatedRule) {
      if (development) {
        window.BlazorStyled.writeRule(sheet, updatedRule, logger);
      } else {
        try {
          if (updatedRule.indexOf(':-moz') !== -1 && 'MozBoxSizing' in document.body.style) {
            window.BlazorStyled.insertRule(sheet.sheet, updatedRule, logger);
          } else if (updatedRule.indexOf(':-moz') === -1) {
            window.BlazorStyled.insertRule(sheet.sheet, updatedRule, logger);
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
  updateRule: function (stylesheetId, stylesheetName, priority, selector, oldRule, rule, development, debug) {
    const logger = window.BlazorStyled.initLogger(debug);
    const sheet = window.BlazorStyled.getOrCreateSheet(stylesheetId, stylesheetName, priority, logger);
    const updatedRule = window.BlazorStyled.parseTheme(stylesheetId, rule, logger);
    if (development) {
      window.BlazorStyled.updateWrittenRule(sheet, oldRule, updatedRule, logger);
    } else {
      try {
        window.BlazorStyled.updateInsertedRule(sheet.sheet, oldRule, updatedRule, logger);
      } catch (err) {
        logger.error('Failed to update: ', rule);
        logger.error(err);
      }
    }
  },
  clearStyles: function (stylesheetId, stylesheetName, debug) {
    const logger = window.BlazorStyled.initLogger(debug);
    const sheet = document.getElementById(stylesheetId);
    if (sheet) {
      document.head.removeChild(sheet);
      logger.log('Cleared stylesheet: ', stylesheetName);
    }
  },
  setThemeValue: function (stylesheetId, stylesheetName, priority, name, value, development, debug) {
    const logger = window.BlazorStyled.initLogger(debug);
    try {
      const theme = window.BlazorStyled.getOrCreateTheme(stylesheetId);
      const oldValue = theme.values[name];
      theme.values[name] = value;
      for (var i in theme.rules) {
        const rule = theme.rules[i];
        if (rule.indexOf(name) !== -1) {
          if (oldValue) {
            const selector = rule.substring(0, rule.indexOf('{'));
            const oldRule = window.BlazorStyled.parseTheme(
              stylesheetId,
              rule.replace('[' + name + ']', oldValue),
              logger
            );
            if (oldRule) {
              window.BlazorStyled.updateRule(
                stylesheetId,
                stylesheetName,
                priority,
                selector,
                oldRule,
                rule,
                development,
                debug
              );
            }
          } else {
            window.BlazorStyled.insertClass(stylesheetId, stylesheetName, priority, rule, development, debug);
          }
        }
      }
    } catch (err) {
      logger.error('Failed to update: ', rule);
      logger.error(err);
    }
  },
  getThemeValues: function (stylesheetId) {
    const theme = window.BlazorStyled.getOrCreateTheme(stylesheetId);
    return theme.values;
  },
  parseTheme: function (stylesheetId, rule, logger) {
    const start = rule.indexOf('{');
    if (rule.indexOf('[', start) === -1) {
      return rule;
    }
    const theme = window.BlazorStyled.getOrCreateTheme(stylesheetId);
    if (!theme.rules.find((r) => r === rule)) {
      theme.rules.push(rule);
    }
    const themeValueName = rule.substring(rule.indexOf('[', start) + 1, rule.indexOf(']', start));
    const themeValue = theme.values[themeValueName];
    if (themeValue === undefined) {
      return undefined;
    }
    const updated = rule.replace('[' + themeValueName + ']', themeValue);
    return window.BlazorStyled.parseTheme(stylesheetId, updated, logger);
  },
  getOrCreateTheme: function (stylesheetId) {
    if (window.BlazorStyled.themes[stylesheetId] === undefined) {
      window.BlazorStyled.themes[stylesheetId] = {
        values: {},
        rules: [],
      };
    }
    return window.BlazorStyled.themes[stylesheetId];
  },
  initLogger: function (debug) {
    if (debug) {
      if (!window.BlazorStyled.debug.init) {
        for (var m in console) {
          if (typeof console[m] === 'function') {
            window.BlazorStyled.debug[m] = console[m].bind(window.console);
          }
        }
        window.BlazorStyled.debug.init = true;
      }
      return window.BlazorStyled.debug;
    } else {
      if (!window.BlazorStyled.fakeDebug.init) {
        for (var m2 in console) {
          if (typeof console[m2] === 'function') {
            window.BlazorStyled.fakeDebug[m2] = function () {};
          }
        }
        window.BlazorStyled.fakeDebug.init = true;
      }
      return window.BlazorStyled.fakeDebug;
    }
  },
  getOrCreateSheet: function (stylesheetId, stylesheetName, priority, logger) {
    const DATA_PRIORITY = 'data-blazorstyled-priority';
    const DATA_NAME = 'data-blazorstyled-name';
    const sheet = document.getElementById(stylesheetId);
    if (sheet) return sheet;
    const styleEl = document.createElement('style');
    const id = document.createAttribute('id');
    id.value = stylesheetId;
    styleEl.setAttributeNode(id);
    const dataName = document.createAttribute(DATA_NAME);
    dataName.value = stylesheetName;
    styleEl.setAttributeNode(dataName);
    const dataPriority = document.createAttribute(DATA_PRIORITY);
    dataPriority.value = priority;
    styleEl.setAttributeNode(dataPriority);
    const head = document.head;
    if (head.hasChildNodes()) {
      let found = false;
      for (let i = 0; i < head.children.length; i++) {
        const node = head.children[i];
        if (node.hasAttribute(DATA_PRIORITY)) {
          const attr = node.getAttribute(DATA_PRIORITY);
          const currentPriority = parseInt(attr, 10);
          if (priority >= currentPriority && !found) {
            found = true;
            if (i !== head.children.length - 1) {
              head.insertBefore(styleEl, head.children[i + 1]);
            } else {
              head.appendChild(styleEl);
            }
          }
        }
      }
      if (!found) {
        head.insertBefore(styleEl, head.firstChild);
      }
    } else {
      head.appendChild(styleEl);
    }
    logger.log('Inserted stylesheet: ', styleEl);
    return styleEl;
  },
  writeRule: function (sheet, rule, logger) {
    if (!sheet.innerText) {
      sheet.innerText = rule;
      logger.log('Written: ', rule);
    } else {
      if (sheet.innerText.indexOf(rule) === -1) {
        sheet.innerText = rule.startsWith('@import') ? rule + sheet.innerText : sheet.innerText + rule;
        logger.log('Written: ', rule);
      }
    }
  },
  insertRule: function (sheet, rule, logger) {
    const index = rule.startsWith('@import') ? 0 : sheet.cssRules.length;
    sheet.insertRule(rule, index);
    logger.log('Inserted at ' + index + ': ', rule);
  },
  updateWrittenRule: function (sheet, oldRule, rule, logger) {
    if (!sheet.innerText) {
      sheet.innerText = rule;
    }
    sheet.innerText = sheet.innerText.replace(oldRule, rule);
    logger.log('Updated old rule: ' + oldRule + ' to new rule: ' + rule);
  },
  updateInsertedRule: function (sheet, oldRule, rule, logger) {
    const oldCssText = window.BlazorStyled.getRuleText(oldRule);
    const newCssText = window.BlazorStyled.getRuleText(rule);
    if(oldCssText !== newCssText) {
        let index = -1;
        for (var i = 0; i < sheet.cssRules.length; i++) {
          if (sheet.cssRules[i].cssText === oldCssText) {
            index = i;
          }
        }
        if (index !== -1) {
          sheet.deleteRule(index);
          sheet.insertRule(rule, index);
          logger.log('Updated old rule at ' + index + ': ' + oldRule + ' to new rule: ' + rule);
        }
    }
  },
  getRuleText: function (rule) {
    const head = document.head;
    const styleEl = document.createElement('style');
    head.appendChild(styleEl);
    styleEl.sheet.insertRule(rule);
    const text = styleEl.sheet.cssRules[0].cssText;
    head.removeChild(styleEl);
    return text;
  },
  themes: {},
  debug: {},
  fakeDebug: {},
};";
    }
}
