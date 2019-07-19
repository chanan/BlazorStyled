var styleEl;
var styleSheet;

window.styledJsFunctions = {
    insertRule: function (rule, development) {
        //console.log(rule);
        if (styleEl === undefined) createStylesheet();
        if (development) {
            var text = styleEl.innerText;
            text = text + rule;
            styleEl.innerText = text;
            return -1;
        } else {
            let num = -1;
            try {
                num = styleSheet.insertRule(rule);
            } catch{
                //ignored
            }
            finally {
                return num;
            }
        }
    },
    clearAllRules: function () {
        const head = document.head;
        head.removeChild(styleEl);
        createStylesheet();
        return true;
    }
};

function createStylesheet() {
    styleEl = document.createElement('style');
    const head = document.head;
    if (head.firstChild) {
        head.insertBefore(styleEl, head.firstChild);
    } else {
        head.appendChild(styleEl);
    }
    styleSheet = styleEl.sheet;
}