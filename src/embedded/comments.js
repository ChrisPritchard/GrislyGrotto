
// comment scrolling
if (window.location.hash == "#comments") {
    Promise.all(Array.from(document.images).filter(img => !img.complete).map(img => new Promise(resolve => { img.onload = img.onerror = resolve; }))).then(() => {
        window.scrollTo(0,document.getElementById('comments_anchor').offsetTop);
    });
}

// Comment form validation:

let comment_count = document.querySelectorAll(".comment_container").length;
if (comment_count >= 20) {
    document.querySelector(".comment_form").remove();
} else {

    // comment honey pot - headless crawlers that dont run js will be unable to post comments
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

// Comment editing

function selectCommentElem(id, name) {
    return document.querySelector("."+name+"[data-comment = '"+id+"']");
}

let ownComments = document.querySelectorAll(".comment_container[data-comment]");
for (let i = 0; i < ownComments.length; i++) {
    let id = ownComments[i].getAttribute("data-comment");

    let comment = ownComments[i];
    let editLink = selectCommentElem(id, "edit");
    let saveLink = selectCommentElem(id, "save");
    let cancelLink = selectCommentElem(id, "cancel");
    let deleteLink = selectCommentElem(id, "delete");
    let existingComment = selectCommentElem(id, "comment_content");
    let errorMessage = selectCommentElem(id, "error");

    editLink.addEventListener("click", function (e) {
        e.preventDefault();

        const xhr = new XMLHttpRequest();
        xhr.open("GET", "/raw_comment/"+id);
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
            editor.classList.add("inline_comment_editor");
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
        
        selectCommentElem(id, "inline_comment_editor").remove();
        existingComment.classList.remove("hide");

        errorMessage.innerText = "";
    });

    saveLink.addEventListener("click", function (e) {
        e.preventDefault();
        
        let editor = selectCommentElem(id, "inline_comment_editor");
        if (editor.value.trim() === "") {
            errorMessage.innerText = "Comments must have some content";
            return;
        } else if (editor.value.trim().length > 1000) {
            errorMessage.innerText = "Comments have a max length of 1000 characters";
            return;
        }

        const xhr = new XMLHttpRequest();
        xhr.open("POST", "/edit_comment/"+id);
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

        xhr.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
        xhr.send("content="+encodeURIComponent(editor.value));
    });

    deleteLink.addEventListener("click", function (e) {
        e.preventDefault();

        if (confirm("are you sure? this is permanant")) {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", "/delete_comment/"+id);
            xhr.onreadystatechange = function() {
                if (xhr.readyState != 4)
                    return;
                if (xhr.status == 202) {
                    comment.remove();
                }
                errorMessage.innerText = "There was an error deleting: " + xhr.response;
            };
            let form = new FormData();
            xhr.send(form);
        }
    });
}

let delete_post = document.querySelector("#delete_post");
if (delete_post) {
    delete_post.addEventListener("click", e => {
        if (!confirm("Are you sure? This action is irreversible!")) {
            e.preventDefault();
            e.stopPropagation();
        }
    });
}
