var styleEl = document.createElement('style');
document.head.appendChild(styleEl);
var styleSheet = styleEl.sheet;

window.styledJsFunctions = {
    insertRule: function (rule, development) {
        if (development) {
            var text = styleEl.innerText;
            text = text + rule;
            styleEl.innerText = text;
            return -1;
        } else {
            return styleSheet.insertRule(rule);
        }
    }
};