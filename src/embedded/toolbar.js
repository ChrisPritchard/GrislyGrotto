
// editor toolbar

let toolbar_buttons = document.querySelectorAll(".editor_toolbar li");
for (let i = 0; i < toolbar_buttons.length; i++) {
    toolbar_buttons[i].addEventListener('click', e => {
        let content = document.querySelector("#content");
        let selected = content.value.substring(content.selectionStart, content.selectionEnd);
        let cap = "";
        if (selected.endsWith(' ')) {
            cap = " ";
        }
        if (e.target.innerText == "B") {
            content.setRangeText("**" + selected.trim() + "**" + cap);
        } else if (e.target.innerText == "I") {
            content.setRangeText("*" + selected.trim() + "*" + cap);
        } else if (e.target.innerText == "🔗") {
            content.setRangeText("[" + selected.trim() + "](https://)" + cap);
        } else {
            let value = e.target.innerText;
            content.setRangeText(value);
        }
    });
}