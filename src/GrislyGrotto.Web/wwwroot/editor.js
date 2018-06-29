
document.querySelector("#isStoryToggle").onclick = function() {
    document.querySelector("#isStory").value = (document.querySelector("#isStory:checked") != null).toString();
}

document.querySelector("#submit").onclick = function() {
    let content = document.querySelector("[name='editmode']:checked").value == "html"
        ? document.querySelector("#editor").innerText : document.querySelector("#editor").innerHTML;
        document.querySelector("#content").value = content;
    return true;
}

let editmodeRadios = document.querySelectorAll("[name='editmode']");
for(var index in editmodeRadios) {
    let radio = editmodeRadios[index];
    radio.onchange = function() {
        let checked = document.querySelector("[name='editmode']:checked").value;
        let editor = document.querySelector("#editor");
        if(checked == "html")
            editor.innerText = editor.innerHTML;
        else
            editor.innerHTML = editor.innerText;
    }
}

var autosaveStatus = document.querySelector("#saving-status");
if(autosaveStatus)
    setInterval(function() {
        
        autosaveStatus.innerText = 'Saving...';

        var request = new XMLHttpRequest();
        request.open('POST', '/api/savework', true);
        request.setRequestHeader('Content-Type', 'application/json');
        request.onload = function() {
            setTimeout(function() {
                autosaveStatus.innerText = 'Saved';
                setTimeout(function() { autosaveStatus.innerText = ''; }, 1000);
            }, 1000);                
        };
        
        let editor = document.querySelector("#editor");
        let checked = document.querySelector("[name='editmode']:checked").value;
        if(checked == "html")
            request.send(JSON.stringify(editor.innerText));
        else
            request.send(JSON.stringify(editor.innerHTML));
    }, 10000);