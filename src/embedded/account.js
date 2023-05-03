
const urlParams = new URLSearchParams(window.location.search);
const message = urlParams.get('message');
if (message) {
    document.querySelector(".account_update_message").innerText = message;
}

let displayNameChange = document.getElementById('display_name_update');
displayNameChange.addEventListener("click", e => {
    let displayName = document.querySelector("#new_display_name");
    let displayNameError = document.querySelector("#display_name_error");

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

let profileImage = document.getElementById('profile_image');
let profileImageChange = document.getElementById('profile_image_update');

profileImage.addEventListener('change', function() {
    let profileImageError = document.querySelector("#profile_image_error");

    if (profileImage.files.length == 0) {
        profileImageError.innerText = "Please select a image to upload";
        profileImageChange.setAttribute("disabled", "disabled");
        return;
    }

    let size = Math.round((profileImage.files[0].size/1024/1024)*100)/100;
    if (size > 1) {
        profileImageError.innerText = "File size is too large ("+size+" MB)";
        profileImageChange.setAttribute("disabled", "disabled");
        return;
    }

    profileImageError.innerText = "";
    profileImageChange.removeAttribute("disabled")
});

profileImageChange.addEventListener("click", _ => {
    let profileImage = document.querySelector("#profile_image");
    fetch("/account/profile_image", {
        method: "POST",
        body: profileImage.files[0]
    }).then(r => {
        if (r.ok) {
            let img = document.querySelector(".profile_image");
            img.src = "/content/" + img.getAttribute("data-username") + "?t=" + new Date().getTime();
            profileImageChange.setAttribute("disabled", "disabled");
        }
        r.text().then(b => document.querySelector(".account_update_message").innerText = b);
    })
});    

let passwordChange = document.getElementById('password_update');
passwordChange.addEventListener("click", e => {
    let oldPassword = document.querySelector("#old_password");
    let newPassword = document.querySelector("#new_password");
    let newPasswordConfirm = document.querySelector("#new_password_confirm");
    let passwordError = document.querySelector("#password_error");

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