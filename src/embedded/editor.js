// title, content and similar key )via title) validation

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

// dirty flag to prevent accidentally moving away from the editor

let dirty = true
document.querySelector("#post_submit").addEventListener("click", () => {
    dirty = false;
});
window.addEventListener("beforeunload", e => {
    if (dirty) {
        // note: most browsers ignore this and just detect that I return anything at all
        // if I do (e.g. the below 'confirm' object), then they present their own version of the below
        // I could return 1 here, or true, or even false, and it would trigger a prompt
        e.returnValue = confirm('Are you sure you want to leave?'); // this seems to work on more modern browsers
        return confirm('Are you sure you want to leave?'); // older browsers
    }
    // by returning nothing here, there is no prompt. note that returning false here WOULD trigger a prompt
});

// content uploader

let contentSelector = document.querySelector('#content_selector');
let upload = document.querySelector('#content_upload_submit');
let result = document.querySelector("#content_upload_result");
let html = document.querySelector("#copy_content_html");

contentSelector.addEventListener('change', function() {
    let files = contentSelector.files;
    if(files.length != 1) {
        return;
    }
    let size = Math.round((files[0].size/1024/1024)*100)/100;
    if (size > 1) {
        result.innerText = "file size is too large ("+size+" MB, max size is 1 MB)";
        html.classList.add("hide");
        upload.classList.add("hide");
        return;
    }
    result.innerText = "";
    upload.classList.remove('hide');
});


upload.addEventListener('click', function() {
    upload.classList.add('hide');
    let files = contentSelector.files;

    let filename = (new Date()).getTime() + "-" + files[0].name;
    filename = filename.toLowerCase().replace(/ /g, '-');

    fetch("/content/"+filename, {
        method: "POST",
        body: files[0]
    }).then(r => {
        if (r.ok) {
            if (filename.endsWith(".zip")) {
                result.innerText = "<a href=\"/content/"+filename+"\">"+filename+"</a>";
            } else if (filename.endsWith(".mp4")) {
                result.innerText = "<div align=\"center\"><video width=\"800\" src=\"/content/"+filename+"\" controls/></div>";
            } else {
                result.innerText = "<div align=\"center\"><a href=\"/content/"+filename+"\" target=\"_blank\"><img style=\"max-width:800px\" src=\"/content/"+filename+"\" /></a></div>";
            }
            html.classList.remove("hide");
        } else {
            result.innerText = "An error occurred uploading :(";
            html.classList.add("hide");
        }
    })

    return false;
});

html.addEventListener('click', function () {
    let textToCopy = result.innerText;
    const temp = document.createElement('textarea');
    temp.value = textToCopy;
    document.body.appendChild(temp);
    temp.select();
    temp.setSelectionRange(0, 99999); // For mobile devices
    navigator.clipboard.writeText(temp.value);
    document.body.removeChild(temp);
});
