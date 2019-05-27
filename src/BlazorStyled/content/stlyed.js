var styleEl = document.createElement('style');
document.head.appendChild(styleEl);
var styleSheet = styleEl.sheet;

window.styledJsFunctions = {
    insertRule: function (rule) {
        var num = styleSheet.insertRule(rule);
        var text = styleEl.innerText;
        text = text + rule;
        styleEl.innerText = text;
        return num;
    }
};