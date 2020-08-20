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

// Confirm elements, e.g. deleting a comment

confirmElems = document.getElementsByClassName("confirm-click");
for (var i = 0; i < confirmElems.length; i++) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
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

var upload = document.querySelector('#content-upload-button');
if  (upload) {
    upload.addEventListener('click', function() {
        var files = document.querySelector('#content-selector').files;
        if(files.length != 1) {
            return false;
        }
        var filename = (new Date()).getTime() + "-" + files[0].name;

        var form = new FormData();
        form.append("file", files[0])

        const xhr = new XMLHttpRequest();
        xhr.open("POST", "/content/"+filename);
        xhr.addEventListener("load", function() {
            document.querySelector("#content-upload-result").innerText = "<img src=\"/content/"+filename+"\" />";
            document.querySelector("#copy-content-html").classList.remove("hide");
        });
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