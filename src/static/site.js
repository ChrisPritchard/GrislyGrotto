let anim = new GrislyGrotto.BackgroundAnimation()
anim.entityCount = 50
anim.initialise(
        document.getElementById('background'), 
        'black', 
        '#9acc14', 
        'grey')

confirmElems = document.getElementsByClassName("confirm-click");
for (var i = 0; i < confirmElems.length; i++) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
    });
}

submitOnceElems = document.getElementsByClassName("submit-once");
for (var i = 0; i < submitOnceElems.length; i++) {
    submitOnceElems[i].addEventListener("click", function () {
        this.disabled = true;
    });
}