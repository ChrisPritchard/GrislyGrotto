let anim = new GrislyGrotto.BackgroundAnimation()
anim.entityCount = 50
anim.initialise(
        document.getElementById('background'), 
        'black', 
        '#9acc14', 
        'grey')

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