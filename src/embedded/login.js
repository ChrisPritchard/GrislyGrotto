
// login honey pot - headless crawlers that dont run js will be unable to post the login form
let category = document.querySelector("#category");
if (category) {
    category.value = "user";
    category.style.display = "none";
}

function test_form_is_valid() {
    let username_len = document.querySelector("#username").value.length;
    let password_len = document.querySelector("#password").value.length;
    if (username_len == 0 || password_len == 0) {
        document.querySelector("#form_submit").setAttribute("disabled", "disabled");
    } else {
        document.querySelector("#form_submit").removeAttribute("disabled");
    }
}

document.querySelector("#username").addEventListener("keyup", () => test_form_is_valid());
document.querySelector("#password").addEventListener("keyup", () => test_form_is_valid());
test_form_is_valid();