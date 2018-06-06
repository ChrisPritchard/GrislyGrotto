document.getElementById("isStoryToggle").onclick = function() {
    document.getElementById("isStory").value = (document.querySelector("#isStory:checked") != null).toString();
}

document.getElementById("submit").onclick = function() {
    let content = document.querySelector("[name='editmode']:checked").value == "html"
        ? document.getElementById("editor").innerText : document.getElementById("editor").innerHTML;
    document.getElementById("content").value = content;
    return true;
}

document.getElementsByName("editmode").forEach(o => o.onclick = function() {
    let checked = document.querySelector("[name='editmode']:checked").value;
    let editor = document.getElementById("editor");
    if(checked == "html")
        editor.innerText = editor.innerHTML;
    else
        editor.innerHTML = editor.innerText;
});