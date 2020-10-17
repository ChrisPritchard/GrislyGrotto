// background animation setup

// - ensure that the canvas always fills the window

function resizeCanvas() {
    let canvas = document.getElementById('animation-left');
    canvas.width = (window.innerWidth-1000)/2;
    canvas.height = window.innerHeight;
    canvas = document.getElementById('animation-right');
    canvas.width = (window.innerWidth-1000)/2;
    canvas.style.left = (window.innerWidth-1000)/2 + 1000 + "px";
    canvas.height = window.innerHeight;
};
resizeCanvas();
window.onresize = function () { resizeCanvas(); };

// - trigger the animation

function getColour(elem) {
    return window.getComputedStyle(elem, null).getPropertyValue("background-color");
}

function initAnimation(canvasId) {
    var animationCanvas = document.getElementById(canvasId);
    var animSettings = wanderingTriangles.baseSettings();
    animSettings.backgroundColour = getColour(document.getElementById('vis-background-colour'));
    animSettings.primaryColour = getColour(document.getElementById('vis-primary-colour'));
    animSettings.secondaryColour = getColour(document.getElementById('vis-secondary-colour'));
    return wanderingTriangles.init(animationCanvas, animSettings);
}

var animationLeft = initAnimation('animation-left');
var animationRight = initAnimation('animation-right');

// visualisation control panel

document.getElementById('vis-enabled').onchange = function() {
    animationLeft.enabled = this.checked;
    animationRight.enabled = this.checked;
}
document.getElementById('site-theme').onchange = function() {
    document.getElementById('current-theme').value = this.value;
}
document.getElementById('site-theme').value = document.getElementById('current-theme').value;

// Confirm elements, e.g. deleting a post

confirmElems = document.getElementsByClassName("confirm-click");
for (var i = 0; i < confirmElems.length; i++) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
    });
}

// Comment editing

function selectCommentLink(id, name) {
    return document.querySelector("."+name+"[data-comment = '"+id+"']");
}

editCommentElems = document.getElementsByClassName("comment-link edit");
for (var i = 0; i < editCommentElems.length; i++) {
    editCommentElems[i].addEventListener("click", function (e) {
        var id = this.getAttribute("data-comment");
        this.classList.add("hide");
        selectCommentLink(id, "cancel").classList.remove("hide");
        selectCommentLink(id, "save").classList.remove("hide");
        
        var editor = document.createElement("textarea");
        editor.setAttribute("rows", 3);
        editor.setAttribute("cols", "50");
        editor.setAttribute("data-comment", id);
        editor.classList.add("inline-comment-editor");
        var existing = selectCommentLink(id, "comment-content");
        editor.innerText = existing.innerText;
        existing.insertAdjacentElement("afterend", editor);
        existing.classList.add("hide");
        e.preventDefault();
    });
}

cancelCommentElems = document.getElementsByClassName("comment-link cancel");
for (var i = 0; i < cancelCommentElems.length; i++) {
    cancelCommentElems[i].addEventListener("click", function (e) {
        var id = this.getAttribute("data-comment");
        this.classList.add("hide");
        selectCommentLink(id, "edit").classList.remove("hide");
        selectCommentLink(id, "save").classList.add("hide");
        
        selectCommentLink(id, "inline-comment-editor").remove();
        selectCommentLink(id, "comment-content").classList.remove("hide");
        e.preventDefault();
    });
}

saveCommentElems = document.getElementsByClassName("comment-link save");
for (var i = 0; i < saveCommentElems.length; i++) {
    saveCommentElems[i].addEventListener("click", function (e) {
        var id = this.getAttribute("data-comment");
        var editor = selectCommentLink(id, "inline-comment-editor");
        // todo check for no content

        const xhr = new XMLHttpRequest();
        xhr.open("POST", "/edit-comment/"+id);
        xhr.onreadystatechange = function() {
            if (xhr.readyState != 4)
                return;
            if (xhr.status != 202) {
                // TODO: some sort of error handler?
                return
            }

            selectCommentLink(id, "save").classList.add("hide");
            selectCommentLink(id, "edit").classList.remove("hide");
            selectCommentLink(id, "cancel").classList.add("hide");
            
            var existing = selectCommentLink(id, "comment-content");
            existing.innerText = editor.value;
            editor.remove();
            existing.classList.remove("hide");
        };
        var form = new FormData();
        form.append("content", editor.value)
        xhr.send(form);

        e.preventDefault();
    });
}

deleteCommentElems = document.getElementsByClassName("comment-link delete");
for (var i = 0; i < deleteCommentElems.length; i++) {
    deleteCommentElems[i].addEventListener("click", function (e) {
        var id = this.getAttribute("data-comment");

        if (confirm("are you sure? this is permanant")) {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", "/delete-comment/"+id);
            xhr.onreadystatechange = function() {
                if (xhr.readyState != 4)
                    return;
                if (xhr.status == 202) {
                    document.querySelector("div[data-comment = '"+id+"']").remove();
                }
                // TODO: some sort of error handler?
            };
            xhr.send();
        }

        e.preventDefault();
    });
}

// Dirty flag and confirm leave on editor page, to stop 'accidents'

let title = document.getElementById('title');
let content = document.getElementById('content');
if (title && content) { // both these being present means the user is on the editor page
    let dirty = false
    title.onchange = function () { dirty = true; }
    content.onchange = function () { dirty = true; }
    document.getElementById('submit').onclick = function () {
        dirty = false;
    }
    window.onbeforeunload = function () {
        if (dirty) {
            // note: most browsers ignore this and just detect that I return anything at all
            // if I do (e.g. the below 'confirm' object), then they present their own version of the below
            // I could return 1 here, or true, or even false, and it would trigger a prompt
            return confirm('are you sure you want to leave?')
        }
        // by returning nothing here, there is no prompt. note that returning false here WOULD trigger a prompt
    }
}

// cookie expiry countdown

if (title && content) {

    function updateTimer(timeRemaining) {
        minutes = Math.floor(timeRemaining / 60);
        labelClass = "timer";
        if (minutes < 10) {
            labelClass += " error";
        }
        seconds = Math.floor(timeRemaining % 60);

        let timer = document.getElementById('timer');
        if (!timer) {
            timer = document.createElement('span');
            timer.id = 'timer';
            document.querySelector('h2').insertAdjacentElement('afterend', timer);
        }
        if (minutes > 0) {
            plural = minutes == 1 ? "" : "s";
            timer.innerText = minutes+" minute"+plural+" remaining";
        } else {
            plural = seconds == 1 ? "" : "s";
            timer.innerText = seconds+" second"+plural+" remaining";
        }
        timer.className = labelClass;
    }

    start = new Date().getTime ();
    cookieTime = 60*60*1000;
    interval = setInterval(function () {
        timeRemaining = (cookieTime-((new Date().getTime())-start))/1000;
        if (timeRemaining <= 0) {
            alert('Your auth cookie has now expired, and you can no longer post this blog.\nPlease copy your content to your clipboard and re-login.');
            document.getElementById('timer').innerText = 'Cookie has expired';
            document.getElementById('submit').style.display = 'none';
            clearInterval(interval);
        } else {
            updateTimer(timeRemaining);
        }
    }, 1000);
}

// content uploader

var contentSelector = document.querySelector('#content-selector');
if  (contentSelector) {
    contentSelector.addEventListener('change', function() {
        var files = contentSelector.files;
        if(files.length != 1) {
            return;
        }
        var size = Math.round((files[0].size/1024/1024)*100)/100;
        if (size > 1) {
            document.querySelector("#content-upload-result").innerText = "file size is too large ("+size+" MB)";
            document.querySelector("#copy-content-html").classList.add("hide");
            return;
        }
        document.querySelector("#content-upload-result").innerText = "";
        upload.classList.remove('hide');
    });

    var upload = document.querySelector('#content-upload-button');
    upload.addEventListener('click', function() {
        upload.classList.add('hide');
        var files = contentSelector.files;

        var filename = (new Date()).getTime() + "-" + files[0].name;

        var form = new FormData();
        form.append("file", files[0])

        const xhr = new XMLHttpRequest();
        xhr.open("POST", "/content/"+filename);
        xhr.onreadystatechange = function() {
            if (xhr.readyState != 4)
                return;
                
            if (xhr.status == 202) {
                document.querySelector("#content-upload-result").innerText = "<div align=\"center\"><img style=\"max-width:800px\" src=\"/content/"+filename+"\" /></div>";
                document.querySelector("#copy-content-html").classList.remove("hide");
            } else {
                document.querySelector("#content-upload-result").innerText = "an error occurred uploading :(";
                document.querySelector("#copy-content-html").classList.add("hide");
            }
        };

        xhr.send(form);
        return false;
    });

    document.querySelector("#copy-content-html").addEventListener('click', function () {
        var textToCopy = document.querySelector("#content-upload-result").innerText;
        const temp = document.createElement('textarea');
        temp.value = textToCopy;
        document.body.appendChild(temp);
        temp.select();
        document.execCommand('copy');
        document.body.removeChild(temp);
    });
}