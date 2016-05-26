$().ready(function()
{
    $('.header').click(function() { window.location = '/'; });
    new nicEditor({ iconsPath: '/resources/niceditor/nicEditorIcons.gif' }).panelInstance('editorContent');
});

function checkLogin()
{
    result = true;
    $.ajax(
    {
        url: '/Action/CheckLogin',
        data:
        {
            LoginName: $('#loginName').val(),
            LoginPassword: $('#loginPassword').val()
        },
        async: false,
        success:
        function(data)
        {
            if (data != 'False')
                return;
            $('#loginError').text('Incorrect Username or Password. Please try again.');
            result = false;
        }
    });
    return result;
}

function setSearchTerm()
{
    $('#searchForm').attr('action', '/Search/' + $('#searchTerm').val());
}

function checkCommentFields()
{
    if ($('#commentAuthor').val().trim() == '' || $('#commentContent').val().trim() == '')
    {
        $('#commentError').text('Comments require an author and some text.');
        return false;
    }
    return true;
}

function checkPostFields()
{
    if ($('#editorTitle').val().trim() == '' 
    || ($('#editorContent').val().trim() == '' && nicEditors.findEditor('editorContent').getContent().trim() == ''))
    {
        $('#editorError').text('Posts require a title and some text.');
        return false;
    }
    return true;
}