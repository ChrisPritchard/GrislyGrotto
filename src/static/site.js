  
alert('test');

confirmElems = document.getElementsByClassName("confirm-click");
for (var i in confirmElems) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
    });
}