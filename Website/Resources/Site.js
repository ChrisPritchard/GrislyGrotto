$().ready(function()
{
    $('.header').click(function() { window.location = '/'; });
    CKEDITOR.config.resize_enabled = false;
});

function saveDraft()
{
    $('#draftStatus').html('Saving draft...')
    $.post('saveDraft.ashx',
    {
        Title: $('#editorTitle').val(),
        Content: CKEDITOR.instances.editorContent.getData(),
        LoggedUser: $('#loggedUser').val()
    },
    function(id)
    {
        $('.editorPost').val(id);
        $('#draftStatus').html('Draft saved');
        setTimeout(function()
        {
            $('#draftStatus').html('<a href="#" onclick="saveDraft()">Save draft</a>');
        }, 1000);
    });
}

function newPost()
{
    $('#draftStatus').show();
    $('#editorPost').val('');
    $('#editorTitle').val('');
    CKEDITOR.instances.editorContent.setData('');
    
    $('.editor').dialog
    ({
        dialogClass: 'editorDialog',
        draggable: false,
        modal: true,
        resizable: false,
        width: 800,
        height: 600
    });
    $('.editor').dialog('open');
}

function editPost(id)
{
    $('#draftStatus').hide();
    $('#editorPost').val(id);
    $('#editorTitle').val($('#postTitle' + id).html());
    CKEDITOR.instances.editorContent.setData($('#postContent' + id).html());

    $('.editor').dialog
    ({
        dialogClass: 'editorDialog',
        draggable: false,
        modal: true,
        resizable: false,
        width: 800,
        height: 400
    });
    $('.editor').dialog('open');
}