let query = document.querySelector;

query("#isStoryToggle").onclick = function() {
    query("#isStory").value = (query("#isStory:checked") != null).toString();
}

query("#submit").onclick = function() {
    let content = query("[name='editmode']:checked").value == "html"
        ? query("#editor").innerText : query("#editor").innerHTML;
    query("#content").value = content;
    return true;
}

let editmodeRadios = document.querySelectorAll("[name='editmode']");
for(var index in editmodeRadios) {
    let radio = editmodeRadios[index];
    radio.onchange = function() {
        let checked = query("[name='editmode']:checked").value;
        let editor = query("#editor");
        if(checked == "html")
            editor.innerText = editor.innerHTML;
        else
            editor.innerHTML = editor.innerText;
    }
}

var autosaveStatus = query("#saving-status");
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
        
        let editor = query("#editor");
        let checked = query("[name='editmode']:checked").value;
        if(checked == "html")
            request.send(JSON.stringify(editor.innerText));
        else
            request.send(JSON.stringify(editor.innerHTML));
    }, 10000);