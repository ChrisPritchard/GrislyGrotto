// current path setting

let path = window.location.pathname.substring(1)
document.getElementById('return-path').value = path;
let loginLink = document.getElementById('login-link');
if (loginLink) {
    loginLink.href = loginLink.href + "?returnUrl=" + path;
}

// background animation setup

let anim = new GrislyGrotto.BackgroundAnimation()
anim.entityCount = 50
currentTheme = JSON.parse(document.getElementById('current-theme').value)
anim.initialise(
        document.getElementById('background'), 
        currentTheme.background, 
        currentTheme.primary, 
        currentTheme.secondary)

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
if (title && content) {
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
        // by returning nothing here, there is no prompt. note that return false here DOES trigger a prompt
    }
}

// visualisation control panel

document.getElementById('vis-enabled').onchange = function() {
    anim.enabled = this.checked;
}
document.getElementById('site-theme').onchange = function() {
    let theme = this.value;
    if (theme === 'green-black') {
        currentTheme.background = "black";
        currentTheme.primary = "#9acc14";
        currentTheme.secondary = "gray";
    } else if (theme === 'red-white') {
        currentTheme.background = "white";
        currentTheme.primary = "red";
        currentTheme.secondary = "maroon";
    } else if (theme === 'gold-black') {
        currentTheme.background = "black";
        currentTheme.primary = "gold";
        currentTheme.secondary = "whitesmoke";
    } else if (theme === 'grey-white') {
        currentTheme.background = "white";
        currentTheme.primary = "Silver";
        currentTheme.secondary = "SlateGray";
    }
    document.getElementById('current-theme').value = JSON.stringify(currentTheme)
    anim.entities = []
    anim.initialise(
        document.getElementById('background'), 
        currentTheme.background, 
        currentTheme.primary, 
        currentTheme.secondary)
}