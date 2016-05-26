$(document).ready(function() {

    $("#Login").validate({
        rules: {
            Username: {
                required: true
            },
            Password: {
                required: true
            }
        },
        messages: {
            Username: {
                required: "Please enter a username"
            },
            Password: {
                required: "Please enter a password"
            }
        }
    });

});