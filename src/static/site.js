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
    let animationCanvas = document.getElementById(canvasId);
    let animSettings = wanderingTriangles.baseSettings();
    animSettings.backgroundColour = getColour(document.getElementById('vis-background-colour'));
    animSettings.primaryColour = getColour(document.getElementById('vis-primary-colour'));
    animSettings.secondaryColour = getColour(document.getElementById('vis-secondary-colour'));
    return wanderingTriangles.init(animationCanvas, animSettings);
}

let animationLeft = initAnimation('animation-left');
let animationRight = initAnimation('animation-right');

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
for (let i = 0; i < confirmElems.length; i++) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
    });
}

// Comment editing

function selectCommentElem(id, name) {
    return document.querySelector("."+name+"[data-comment = '"+id+"']");
}

let ownComments = document.querySelectorAll(".comment-container[data-comment]");
for (let i = 0; i < ownComments.length; i++) {
    let id = ownComments[i].getAttribute("data-comment");

    let comment = ownComments[i];
    let editLink = selectCommentElem(id, "edit");
    let saveLink = selectCommentElem(id, "save");
    let cancelLink = selectCommentElem(id, "cancel");
    let deleteLink = selectCommentElem(id, "delete");
    let existingComment = selectCommentElem(id, "comment-content");
    let errorMessage = selectCommentElem(id, "error");

    editLink.addEventListener("click", function (e) {
        e.preventDefault();

        const xhr = new XMLHttpRequest();
        xhr.open("GET", "/raw-comment/"+id);
        xhr.onreadystatechange = function() {
            if (xhr.readyState != 4)
                return;
            if (xhr.status != 200) {
                errorMessage.innerText = "There was an error getting the comment source: " + xhr.response;
                return
            }

            editLink.classList.add("hide");
            cancelLink.classList.remove("hide");
            saveLink.classList.remove("hide");
            
            let editor = document.createElement("textarea");
            editor.setAttribute("rows", 3);
            editor.setAttribute("cols", "50");
            editor.setAttribute("maxlength", "1000");
            editor.setAttribute("data-comment", id);
            editor.classList.add("inline-comment-editor");
            editor.innerHTML = xhr.responseText;

            existingComment.insertAdjacentElement("afterend", editor);
            existingComment.classList.add("hide");
        };

        xhr.send()
    });

    cancelLink.addEventListener("click", function (e) {
        e.preventDefault();

        cancelLink.classList.add("hide");
        editLink.classList.remove("hide");
        saveLink.classList.add("hide");
        
        selectCommentElem(id, "inline-comment-editor").remove();
        existingComment.classList.remove("hide");

        errorMessage.innerText = "";
    });

    saveLink.addEventListener("click", function (e) {
        e.preventDefault();

        let editor = selectCommentElem(id, "inline-comment-editor");
        if (editor.value.trim() === "") {
            errorMessage.innerText = "Comments must have some content";
            return;
        }

        const xhr = new XMLHttpRequest();
        xhr.open("POST", "/edit-comment/"+id);
        xhr.onreadystatechange = function() {
            if (xhr.readyState != 4)
                return;
            if (xhr.status != 202) {
                errorMessage.innerText = "There was an error saving: " + xhr.response;
                return
            }

            saveLink.classList.add("hide");
            editLink.classList.remove("hide");
            cancelLink.classList.add("hide");
            
            existingComment.innerHTML = xhr.responseText;
            existingComment.classList.remove("hide");
            editor.remove();
            errorMessage.innerText = "";
        };

        let form = new FormData();
        form.append("content", editor.value)
        xhr.send(form);
    });

    deleteLink.addEventListener("click", function (e) {
        e.preventDefault();
        
        if (confirm("are you sure? this is permanant")) {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", "/delete-comment/"+id);
            xhr.onreadystatechange = function() {
                if (xhr.readyState != 4)
                    return;
                if (xhr.status == 202) {
                    comment.remove();
                }
                errorMessage.innerText = "There was an error deleting: " + xhr.response;
            };
            xhr.send();
        }
    });
}

// Account details client validations

function removeSuccessMessages() {
    let elems = document.querySelectorAll(".success-message");
    for(let i = 0; i < elems.length; i++) {
        elems[i].remove();
    }
}

let displayNameChange = document.getElementById('displayName-update');
if (displayNameChange) 
displayNameChange.addEventListener("click", function(e) {
        removeSuccessMessages();
        let displayName = document.querySelector("#displayName");
        let displayNameError = document.querySelector("#displayName-error");

        if (!displayName.value) {
            e.preventDefault();
            displayNameError.innerText = "A blank display name is invalid";
            return;
        }

        if (displayName.value.length > 30) {
            e.preventDefault();
            displayNameError.innerText = "Display names must be 30 characters or less";
            return;
        }
    });

let profileImageChange = document.getElementById('profileImage-update');
if (profileImageChange) 
    profileImageChange.addEventListener("click", function(e) {
        removeSuccessMessages();
        let profileImage = document.querySelector("#profileImage");
        let profileImageError = document.querySelector("#profileImage-error");

        if (profileImage.files.length == 0) {
            e.preventDefault();
            profileImageError.innerText = "Please select a image to upload";
            return;
        }

        let size = Math.round((profileImage.files[0].size/1024/1024)*100)/100;
        if (size > 1) {
            e.preventDefault();
            profileImageError.innerText = "File size is too large ("+size+" MB)";
            return;
        }
    });
    

let passwordChange = document.getElementById('password-update');
if (passwordChange) 
    passwordChange.addEventListener("click", function(e) {
        removeSuccessMessages();
        let oldPassword = document.querySelector("#oldPassword");
        let newPassword = document.querySelector("#newPassword");
        let newPasswordConfirm = document.querySelector("#newPasswordConfirm");
        let passwordError = document.querySelector("#password-error");

        if (!oldPassword.value || !newPassword.value || !newPasswordConfirm.value) {
            e.preventDefault();
            passwordError.innerText = "All values must be provided";
            return;
        }

        if (newPassword.value != newPasswordConfirm.value) {
            e.preventDefault();
            passwordError.innerText = "New password does not match confirm field";
            return;
        }

        if (newPassword.value.length < 14) {
            e.preventDefault();
            passwordError.innerText = "New password must be at least 14 characters long";
            return;
        }
    });

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
            return confirm('Are you sure you want to leave?')
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

let contentSelector = document.querySelector('#content-selector');
if  (contentSelector) {
    contentSelector.addEventListener('change', function() {
        let files = contentSelector.files;
        if(files.length != 1) {
            return;
        }
        let size = Math.round((files[0].size/1024/1024)*100)/100;
        if (size > 1) {
            document.querySelector("#content-upload-result").innerText = "file size is too large ("+size+" MB)";
            document.querySelector("#copy-content-html").classList.add("hide");
            return;
        }
        document.querySelector("#content-upload-result").innerText = "";
        upload.classList.remove('hide');
    });

    let upload = document.querySelector('#content-upload-button');
    upload.addEventListener('click', function() {
        upload.classList.add('hide');
        let files = contentSelector.files;

        let filename = (new Date()).getTime() + "-" + files[0].name;

        let form = new FormData();
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
                document.querySelector("#content-upload-result").innerText = "An error occurred uploading :(";
                document.querySelector("#copy-content-html").classList.add("hide");
            }
        };

        xhr.send(form);
        return false;
    });

    document.querySelector("#copy-content-html").addEventListener('click', function () {
        let textToCopy = document.querySelector("#content-upload-result").innerText;
        const temp = document.createElement('textarea');
        temp.value = textToCopy;
        document.body.appendChild(temp);
        temp.select();
        document.execCommand('copy');
        document.body.removeChild(temp);
    });
}