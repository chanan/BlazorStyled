window.BlazorStyled = {
    insertRule: function (stylesheetId, stylesheetName, rule, development, debug) {
        const logger = initDebug(debug);
        const sheet = getOrCreateSheet(stylesheetId, stylesheetName, logger);
        if (development) {
            writeRule(sheet, rule, logger);
        } else {
            try {
                if (rule.indexOf(':-moz') !== -1 && 'MozBoxSizing' in document.body.style) {
                    insertRule(sheet.sheet, rule, logger);
                }
                else if (rule.indexOf(':-moz') === -1) {
                    insertRule(sheet.sheet, rule, logger);
                } else {
                    logger.warn('Mozilla rule not inserted: ', rule);
                }
            } catch (err) {
                logger.error('Failed to insert: ', rule);
                logger.error(err);
            }
        }
    },
    updateRule: function (stylesheetId, stylesheetName, selector, oldRule, rule, development, debug) {
        const logger = initDebug(debug);
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
        const logger = initDebug(debug);
        const sheet = document.getElementById(stylesheetId);
        if (sheet) {
            document.head.removeChild(sheet);
            logger.log('Cleared stylesheet: ', stylesheetName);
        }
    }
}

function initDebug(debug) {
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
        head.insertBefore(styleEl, head.firstChild);
    } else {
        head.appendChild(styleEl);
    }
    logger.log('Inserted stylesheet: ', styleEl);
    return styleEl;
}

function writeRule(sheet, rule, logger) {
    if (!sheet.innerText) {
        sheet.innerText = rule;
    }
    sheet.innerText = rule.startsWith('@import') ? rule + sheet.innerText : sheet.innerText + rule;
    logger.log('Written: ', rule);
}

function insertRule(sheet, rule, logger) {
    const index = rule.startsWith('@import') ? 0 : sheet.cssRules.length;
    sheet.insertRule(rule, index);
    logger.log('Inserted at ' + index + ': ', rule);
}

function updateWrittenRule(sheet, oldRule, rule, logger) {
    if (!sheet.innerText) {
        sheet.innerText = rule;
    }
    sheet.innerText = sheet.innerText.replace(oldRule, rule);
    logger.log('Updated old rule: ' + oldRule + ' to new rule: ' + rule);
}

function updatedInsertedRule(sheet, oldRule, rule, logger) {
    const temp = getOrCreateSheet('temp', 'temp', initDebug(false));
    temp.sheet.insertRule(oldRule);
    const oldCssText = temp.sheet.cssRules[0].cssText;
    document.head.removeChild(temp);
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