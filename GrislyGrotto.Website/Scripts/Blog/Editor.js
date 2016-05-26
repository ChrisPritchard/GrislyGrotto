$(document).ready(function() {

    $("#CreateBlog").validate({
        rules: {
            Title: {
                required: true
            },
            Content: {
                required: true
            }
        },
        messages: {
            Title: {
                required: "Please enter a title"
            },
            Content: {
                required: "Please enter some content"
            }
        }
    });

    $('#ImageForm').ajaxForm(function() { AllUserImages(true); });
});

function AllUserImages(show) {
    if ($('#UserImagesLink').text() == 'Show All User Image Paths' || show == true) {
        $.ajax({
            url: '/Blog/AllUserImages',
            cache: false,
            dataType: 'json',
            success: function(data) {
                $('#UserImagePaths').html('');
                for (i = 0; i < data.length; i++) {
                    $('#UserImagePaths').append('' + data[i] + '&#160;<a href="' + data[i] + '">(show)</a>');
                    if (i != data.length - 1)
                        $('#UserImagePaths').append('<br/>');
                }
            } 
        });
        $('#UserImagesLink').text('Hide All User Image Paths');
    }
    else {
        $('#UserImagePaths').html('');
        $('#UserImagesLink').text('Show All User Image Paths');
    }
}
