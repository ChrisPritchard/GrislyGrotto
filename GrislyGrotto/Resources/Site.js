
function GrislyGrotto() {
    this.userCredentials = null;
    this.editorViewType = 'normal';

    this.postTemplate = null, this.commentsTemplate = null, this.authenticationTemplate = null, this.editorTemplate = null;

    this.downloadTemplateAsync = function(title, onDownloaded) {
        $.ajax({
            method: 'GET',
            async: false,
            url: '/resources/templates/' + title + '.template.htm?v=10.2',
            success: function (data) { onDownloaded($.templates(data)); }
        });
    }

    this.downloadTemplate = function (title, onDownloaded) {
        $.get('/resources/templates/' + title + '.template.htm',
            function (data) { onDownloaded($.templates(data)); });
    }

    this.getParameterByName = function(name) {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    }

    this.showAuthenticationZone = function(trigger, postId) {
        $('.authenticationZone').remove();
        $(trigger).after(grislygrotto.authenticationTemplate.render());
        $(trigger).hide();

        $('.checkedForValidCredentials').keyup(function () {
            if ($('.username').val() != '' && $('.password').val() != '')
                $('.authenticate').removeAttr('disabled');
            else
                $('.authenticate').attr('disabled', 'disabled');
        });

        $('.authenticate').click(function () {
            grislygrotto.checkAuthentication(postId);
        });
    }

    this.checkAuthentication = function (postId) {

        grislygrotto.userCredentials = {
            credentials: {
                Username: $('.username').val(),
                Password: $('.password').val()
            }
        };

        var methodUrl = '/Blog.svc/';
        if (postId)
            methodUrl += 'CheckAuthenticationForPost/' + postId;
        else
            methodUrl += 'CheckAuthentication';

        $.ajax({
            type: "POST",
            url: methodUrl,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(grislygrotto.userCredentials),
            dataType: "json",
            success: function (result) {
                $('.authenticationZone').remove();
                if (!result) {
                    $('.authenticationError').text('Authentication failed, please try again');
                    return;
                }

                if (postId)
                    $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(), function (data) { grislygrotto.renderEditor(postId, data); });
                else
                    grislygrotto.renderEditor(postId, { Title: '', Content: '' });
            }
        });
    }

    this.loadLatest = function() {
        $.getJSON('/Blog.svc/LatestPosts?r=' + Math.random(),
        function (data) { grislygrotto.renderPosts(data); });
    }

    this.loadPost = function(postId, scrollToComments) {
        $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(),
        function (data) {
            grislygrotto.renderPosts(data);

            $('.posts').append(grislygrotto.commentsTemplate.render(data));
            $('.commentSubmitButton').click(function () { grislygrotto.submitComment($(this).attr('name')); });

            $('.checkedForValidComment').keyup(function () {
                if ($('.commentAuthor').val() != '' && $('.commentText').val() != '')
                    $('.commentSubmitButton').removeAttr('disabled');
                else
                    $('.commentSubmitButton').attr('disabled', 'disabled');
            });

            $('.editPostLink').click(function () {
                grislygrotto.showAuthenticationZone(this, postId);
                return false;
            });

            if (scrollToComments)
                document.getElementById('commentsBegin').scrollIntoView(true);
        });
    }

    this.renderPosts = function(data) {
        $('.posts').html(grislygrotto.postTemplate.render(data));
        $('.postLink').click(function () {
            grislygrotto.loadPost($(this).attr('name'), true);
            return false;
        });
    }

    this.loadArchives = function(template) {
        $.getJSON('/Blog.svc/Archives?r=' + Math.random(),
            function (data) {
                $('.archives').append(template.render(data));
                $('.archives').change(function () {
                    var segments = $(this).val().split(',');
                    $('.stories').val('/');
                    $.getJSON('/Blog.svc/Month/' + segments[0] + '/' + segments[1] + '?r=' + Math.random(),
                        function (data) { grislygrotto.renderPosts(data); });
                });
            });
    }

    this.loadStories = function(template) {
        $.getJSON('/Blog.svc/Stories?r=' + Math.random(),
            function (data) {
                $('.stories').append(template.render(data));
                $('.stories').change(function () {
                    $('.archives').val('/');
                    grislygrotto.loadPost($(this).val());
                });
            });
    }

    this.renderEditor = function (postId, post) {
        $.getJSON('/Blog.svc/GetEditorState?r=' + Math.random(),
            function (data) {
                if (data != null) {
                    if (!post)
                        post = data;
                    else {
                        post.Title = data.Title;
                        post.Content = data.Content;
                    }
                }

                $('.posts').html(grislygrotto.editorTemplate.render(post));

                if (postId) {
                    post.isEditorMode = true;
                    $('.posts').append(grislygrotto.commentsTemplate.render(post));
                }

                $('.viewType').change(function () { grislygrotto.changeViewType(); });
                $('.submitPost').click(function () {
                    grislygrotto.addOrEditPost(postId);
                    return false;
                });

                $('.deleteCommentLink').click(function () {
                    if (confirm('Are you sure you want to delete this comment?')) {
                        var trigger = $(this);
                        $.post('/Blog.svc/DeleteComment/' + $(this).attr('href'),
                            function () { $(trigger).parent().remove(); });
                    }
                    return false;
                });

                $('.deletePostLink').click(function () {
                    if (confirm('Are you sure you want to delete this post?')) {
                        $.ajax({
                            type: "POST",
                            url: '/Blog.svc/DeletePost',
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({ request: { Credentials: grislygrotto.userCredentials.credentials, Id: postId} }),
                            dataType: "json",
                            success: function () { document.location.reload(true); }
                        });
                    }
                    return false;
                });

                setTimeout(function () { grislygrotto.saveEditorState(); }, 5000);
            });
    }

    this.saveEditorState = function() {
        if ($('.editor').length == 0)
            return;

        $('.editorState').html('Saving...');

        var postRequest = {
            post: {
                Title: $('.title').text(),
                Content: $('.content').html(),
                Type: 'Normal'
            }
        };

        if (grislygrotto.editorViewType == 'html')
            postRequest.post.Content = $('.content').text();
        if ($('.isStory').attr('checked'))
            postRequest.post.Type = 'Story';

        $.ajax({
            type: "POST",
            url: '/Blog.svc/SaveEditorState',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(postRequest),
            dataType: "json",
            success: function () { setTimeout(function () { $('.editorState').html('Saved'); }, 500); }
        });

        setTimeout(function () { grislygrotto.saveEditorState(); }, 5000);
    }

    this.changeViewType = function() {
        if (grislygrotto.editorViewType == 'normal') {
            grislygrotto.editorViewType = 'html';
            $('.content').text($('.content').html());
        }
        else {
            grislygrotto.editorViewType = 'normal';
            $('.content').html($('.content').text());
        }
    }

    this.addOrEditPost = function(postId) {
        var postRequest = {
            request: {
                Credentials: grislygrotto.userCredentials.credentials,
                Post: {
                    Title: $('.title').text(),
                    Content: $('.content').html(),
                    Type: 'Normal'
                }
            }
        };

        if (grislygrotto.editorViewType == 'html')
            postRequest.request.Post.Content = $('.content').text();
        if ($('.isStory').attr('checked'))
            postRequest.request.Post.Type = 'Story';
        if (postId)
            postRequest.request.Post.Id = postId;

        $.ajax({
            type: "POST",
            url: '/Blog.svc/AddOrEditPost',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(postRequest),
            dataType: "json",
            success: function () { document.location.reload(true); }
        });
    }

    this.submitComment = function(postId) {
        $('.commentAuthor').attr('disabled', 'disabled');
        $('.commentText').attr('disabled', 'disabled');
        $('.commentSubmitButton').attr('disabled', 'disabled');

        var newComment = { 
            comment: {
                Author: $('.commentAuthor').val(),
                Text: $('.commentText').val()
            }
        };

        $.ajax({
            type: "POST",
            url: '/Blog.svc/AddComment/' + postId,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(newComment),
            dataType: "json",
            success: function () { grislygrotto.loadPost(postId); }
        });
    }

    this.Run = function () {
        this.downloadTemplateAsync('post', function (template) {
            grislygrotto.postTemplate = template;
            var id = grislygrotto.getParameterByName('id')
            if (id != null)
                grislygrotto.loadPost(id);
            else {
                $.ajax({
                    method: 'GET',
                    async: false,
                    url: '/Blog.svc/LatestPosts?r=' + Math.random(),
                    success: function (data) { grislygrotto.renderPosts(data); }
                });
            }
        });

        this.downloadTemplate('comments', function (template) { grislygrotto.commentsTemplate = template; });
        this.downloadTemplate('authentication', function (template) {
            grislygrotto.authenticationTemplate = template;

            $('.newPostLink').click(function () {
                grislygrotto.showAuthenticationZone(this);
                return false;
            });
        });
        this.downloadTemplate('editor', function (template) { grislygrotto.editorTemplate = template; });

        this.downloadTemplate('archive', function (template) { grislygrotto.loadArchives(template); });
        this.downloadTemplate('story', function (template) { grislygrotto.loadStories(template); });

        this.downloadTemplateAsync('quote', function (template) {
            $.ajax({
                method: 'GET',
                async: false,
                url: '/Blog.svc/Quote?r=' + Math.random(),
                success: function (data) { $('.quote').html(template.render(data)); }
            });
        });

        $('.header').click(function () {
            $('.archives').val('/');
            $('.stories').val('/');
            $('.newPostLink').show();
            grislygrotto.loadLatest();
        });

        $('.searchButton').click(function () {
            $.getJSON('/Blog.svc/Search/' + $('.searchTerm').val() + '?r=' + Math.random(),
                function (data) { grislygrotto.renderPosts(data); });
        });
    }
}

var grislygrotto = grislygrotto || new GrislyGrotto();

$().ready(function () {
    grislygrotto.Run();
});