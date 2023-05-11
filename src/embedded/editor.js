let title_is_valid = false;
let current_title = document.querySelector("#title").value;
if (current_title.length == 0) { // new post, enable title validation
    document.querySelector("#title").addEventListener("blur", () => check_title());
} else {
    title_is_valid = true;
}

function test_post_is_valid() {
    if (!title_is_valid) {
        document.querySelector("#post_submit").setAttribute("disabled", "disabled");
        return;
    }
    let title_len = document.querySelector("#title").value.length;
    let content_len = document.querySelector("#content").value.length;
    if (title_len == 0 || content_len == 0 || content_len < 500) {
        document.querySelector("#post_submit").setAttribute("disabled", "disabled");
    } else {
        document.querySelector("#post_submit").removeAttribute("disabled");
    }
}

document.querySelector("#title").addEventListener("keyup", () => test_post_is_valid());
document.querySelector("#content").addEventListener("keyup", () => test_post_is_valid());
test_post_is_valid();

function check_title() {
    let title = document.querySelector("#title").value;
    fetch("/editor/check_title", { 
        method: "POST", 
        headers: { "Content-Type": "application/x-www-form-urlencoded" }, 
        body: "title="+encodeURIComponent(title) 
    }).then(r => r.text()).then(t => {
        title_is_valid = t !== "true";
        if (title_is_valid) {
            document.querySelector("#title_error").classList.add("hide");
        } else {
            document.querySelector("#title_error").classList.remove("hide");
        }
        test_post_is_valid();
    });
}


let dirty = true
document.querySelector("#post_submit").addEventListener("click", () => {
    dirty = false;
});
window.addEventListener("beforeunload", () => {
    if (dirty) {
        // note: most browsers ignore this and just detect that I return anything at all
        // if I do (e.g. the below 'confirm' object), then they present their own version of the below
        // I could return 1 here, or true, or even false, and it would trigger a prompt
        return confirm('Are you sure you want to leave?')
    }
    // by returning nothing here, there is no prompt. note that returning false here WOULD trigger a prompt
});