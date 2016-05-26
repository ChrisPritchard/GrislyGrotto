$().ready(function ()
{
    try
    {
        new nicEditor({ iconsPath: '/resources/niceditor/nicEditorIcons.gif' }).panelInstance('Editor');
    } catch (e) { }

    $('.header h1').click(function ()
    {
        document.location.href = '/';
    });

    $('#loginLink a').click(function ()
    {
        $('#loginPanel').show();
        $('#loginLink').hide();
        return false;
    });

    currentUrl = $('#currentUrl').val();
    $('#currentUrl').remove();
    $('option').each(function ()
    {
        if ($(this).val() == currentUrl)
            $(this).attr('selected', 'true');
    });

    $('select').change(function ()
    {
        document.location.href = $(this).val();
    });

    $('input[value="Login"]').click(function ()
    {
        if ($('[name="Username"]').val().trim() == '' || $('[name="Password"]').val().trim() == '')
        {
            if (!shownLoginError)
            {
                if ($('#loginPanel p[class="error"]').length)
                    $('#loginPanel p[class="error"]').text('Both username and password required');
                else
                    $('input[value="Login"]').before('<p class="error">Both username and password required</p>');
                shownLoginError = true;
            }
            return false;
        }

        return true;
    });

    if ($('#loginPanel p[class="error"]').length)
    {
        $('#loginPanel').show();
        $('#loginLink').hide();
    }

    if ($('input[value="Add Comment"]').length)
    {
        $('input[value="Add Comment"]').click(function ()
        {
            if ($('[name="Author"]').val().trim() == '' || $('[name="Content"]').val().trim() == '')
            {
                if (!shownContentError)
                {
                    $('input[value="Add Comment"]').before('<p class="error">Both author and content required</p>');
                    shownContentError = true;
                }
                return false;
            }

            return true;
        });
    }

    if ($('input[value="Submit"]').length)
    {
        $('input[value="Submit"]').click(function ()
        {
            if ($('[name="Title"]').val().trim() == '' || ($('[name="Content"]').val().trim() == '' && nicEditors.findEditor('Editor').getContent().trim().length < 10))
            {
                if (!shownContentError)
                {
                    $('input[value="Submit"]').before('<p class="error">Both title and content required</p>');
                    shownContentError = true;
                }
                return false;
            }

            return true;
        });
    }
});

shownLoginError = false;
shownContentError = false;