
function removeSuccessMessages() {
    let elems = document.querySelectorAll(".success_message");
    for(let i = 0; i < elems.length; i++) {
        elems[i].remove();
    }
}

let displayNameChange = document.getElementById('display_name_update');
if (displayNameChange) 
displayNameChange.addEventListener("click", e => {
        removeSuccessMessages();
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

let profileImageChange = document.getElementById('profile_image_update');
if (profileImageChange) 
    profileImageChange.addEventListener("click", e => {
        removeSuccessMessages();
        let profileImage = document.querySelector("#profile_image");
        let profileImageError = document.querySelector("#profile_image_error");

        if (profileImage.files.length == 0) {
            e.preventDefault();
            profileImageError.innerText = "Please select a image to upload";
            return;
        }

        let size = Math.round((profileImage.files[0].size/1024/1024)*100)/100;
        if (size > 1) {
            e.preventDefault();
            profileImageError.innerText = "File size is too large ("+size+" MB)";
            return;
        }
    });
    

let passwordChange = document.getElementById('password_update');
if (passwordChange) 
    passwordChange.addEventListener("click", e => {
        removeSuccessMessages();
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