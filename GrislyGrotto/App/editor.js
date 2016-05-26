/// <reference path="../Scripts/jquery-2.0.1.js" />
/// <reference path="../Scripts/jquery.form.js" />

$().ready(function () {

    $('.js_delete').click(function() {
        return confirm('Are you sure you want to delete this post? This action is irreversible.');
    });

    $('input[name="ViewType"]').on('change', function () {
        var editor = $('.js_content');
        var currentViewType = $('input[name="ViewType"]:checked').val();

        if (currentViewType === 'Normal')
            editor.html(editor.text());
        else
            editor.text(editor.html());
    });

    $('input[type="submit"]').on('click', function () {

        function getHidden(name, value) {
            return $('<input type="hidden" />').attr('name', name).attr('id', name).val(value);
        }

        $('.js_title').after(getHidden('Title', $('.js_title').text()));

        var currentViewType = $('input[name="ViewType"]:checked').val();
        if (currentViewType === 'Normal')
            $('.js_content').after(getHidden('Content', $('.js_content').html()));
        else
            $('.js_content').after(getHidden('Content', $('.js_content').text()));

        var addedTags = grislyGrotto_addedTags || [];
        if (addedTags.length > 0)
            for (var i = 0; i < grislyGrotto_addedTags.length; i++) {
                var tagClean = grislyGrotto_addedTags[i].name.replace(/ /g, '_');
                $('.js_content').after(getHidden('SelectedTags[' + i + ']', tagClean));
            }

        return true;
    });

    function saveEditorState() {
        $('.js_savingStatus').text('Saving...');

        var currentViewType = $('input[name="ViewType"]:checked').val();
        var content = $('.js_content').html();
        if (currentViewType !== 'Normal')
            content = $('.js_content').text();

        $.ajax({
            method: 'POST',
            url: '/posts/saveState',
            data: {
                ID: $('input[name="ID"]').val(),
                Title: $('.js_title').text(),
                Content: content,
                IsStory: $('#IsStory:checked').length > 0
            },
            dataType: 'json',
            success: function () {
                setTimeout(function() { $('.js_savingStatus').text('Saved'); }, 500);
            },
            error: function() { $('.js_savingStatus').text('Failed'); }
        });

        setTimeout(function () { saveEditorState(); }, 5000);
    }
    setTimeout(function () { saveEditorState(); }, 5000);

    $('.js_deleteComment').on('click', function () {
        if (!confirm('Are you sure you want to delete this comment?'))
            return;

        var that = this;
        $.ajax({
            method: 'POST',
            url: '/comments/deleteComment',
            data: {
                ID: $(this).attr('name')
            },
            dataType: 'json',
            success: function () {
                $(that).parents('.js_comment').remove();
            }
        });
    });

    $('.js_imageEncodingForm').ajaxForm({
        success: function (data) {
            $('.js_encodeResult').val(data);
            $('.js_encodeResult').show();
        }
    });
});




