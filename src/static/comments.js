
let comment_count = document.querySelectorAll(".comment_container").length;
if (comment_count >= 20) {
    document.querySelector("#comment_form").remove();
} else {

    // comment honey pot
    let category = document.querySelector("#category");
    if (category) {
        category.value = "user";
        category.style.display = "none";
    }

    function test_comment_is_valid() {
        let author_len = document.querySelector("#author").value.length;
        let content_len = document.querySelector("#content").value.length;
        if (author_len == 0 || content_len == 0 || content_len > 1000) {
            document.querySelector("#comment_submit").setAttribute("disabled", "disabled");
        } else {
            document.querySelector("#comment_submit").removeAttribute("disabled");
        }
    }

    document.querySelector("#author").addEventListener("keyup", () => test_comment_is_valid());
    document.querySelector("#content").addEventListener("keyup", () => test_comment_is_valid());
    test_comment_is_valid();
}