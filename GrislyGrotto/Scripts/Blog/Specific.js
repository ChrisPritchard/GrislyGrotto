$(document).ready(function() {

    $("#CreateComment").validate({
        rules: {
            Author: {
                required: true
            },
            Content: {
                required: true
            }
        },
        messages: {
            Author: {
                required: "Please enter a name for yourself"
            },
            Content: {
                required: "Please enter some content"
            }
        }
    });

});