/// <reference path="jquery.d.ts" />

class Credentials {
    Username: string;
    Password: string;

    constructor(username: string, password: string) {
        this.Username = username;
        this.Password = password;
    }
}

class Post {
    Id: number;
    Title: string;
    Content: string;
    Type: string;

    constructor(title: string, content: string, type: string) {
        this.Title = title;
        this.Content = content;
        this.Type = type;
    }
}

interface Template {
    render(): string;
    render(data): string;
}

class MainBlogControl {
    private static userCredentials: Credentials;
    private static editorViewType: string;

    private static postTemplate: Template;
    private static commentsTemplate: Template;
    private static  authenticationTemplate: Template;
    private static editorTemplate: Template;

    private static getParameterByName(name: string): string {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    }

    private static downloadTemplateAsync(title: string, onDownloaded: (data: Template) => void) {
        $.ajax({
            method: 'GET',
            async: false,
            url: '/resources/templates/' + title + '.template.htm?v=10.4',
            success: function (data) { onDownloaded($.templates(data)); }
        });
    }

    private static downloadTemplate(title: string, onDownloaded: (data: Template) => void ) {
        $.get('/resources/templates/' + title + '.template.htm?v=10.4',
            function (data) { onDownloaded($.templates(data)); });
    }

    private static postJson(url: string, data: any, onSuccess: (string) => void ) {
        $.ajax({
            type: "POST",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            dataType: "json",
            success: function (result) { onSuccess(result); }
        });
    }

    private static showAuthenticationZone(trigger: any, postId?: number) {
        $('.authenticationZone').remove();
        $(trigger).after(authenticationTemplate.render());
        $(trigger).hide();

        $('.checkedForValidCredentials').keyup(function () {
            if ($('.username').val() != '' && $('.password').val() != '')
                $('.authenticate').removeAttr('disabled');
            else
                $('.authenticate').attr('disabled', 'disabled');
        });

        $('.authenticate').click(function () {
            checkAuthentication(postId);
        });
    }

    private static checkAuthentication(postId?: number) {
        userCredentials = new Credentials($('.username').val(), $('.password').val());

        var methodUrl = '/Blog.svc/';
        if (postId)
            methodUrl += 'CheckAuthenticationForPost/' + postId;
        else
            methodUrl += 'CheckAuthentication';

        postJson(methodUrl, userCredentials, function (result) {
                $('.authenticationZone').remove();
                if (!result) {
                    $('.authenticationError').text('Authentication failed, please try again');
                    return;
                }

                if (postId)
                    $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(), function (data) { renderEditor(postId, data); });
                else
                    renderEditor(postId, { Title: '', Content: '' });
            });
    }

    private static loadLatest() {
        $.getJSON('/Blog.svc/LatestPosts?r=' + Math.random(),
        function (data) { renderPosts(data); });
    }

    private static loadPost(postId: number, scrollToComments?: bool) {
        $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(),
        function (data) {
            renderPosts(data);

            $('.posts').append(commentsTemplate.render(data));
            $('.commentSubmitButton').click(function () { submitComment(parseInt($(this).attr('name'))); });

            $('.checkedForValidComment').keyup(function () {
                if ($('.commentAuthor').val() != '' && $('.commentText').val() != '')
                    $('.commentSubmitButton').removeAttr('disabled');
                else
                    $('.commentSubmitButton').attr('disabled', 'disabled');
            });

            $('.editPostLink').click(function () {
                showAuthenticationZone(this, postId);
                return false;
            });

            if (scrollToComments)
                document.getElementById('commentsBegin').scrollIntoView(true);
        });
    }

    private static renderPosts(data) {
        $('.posts').html(postTemplate.render(data));
        $('.postLink').click(function () {
            loadPost(parseInt($(this).attr('name')), true);
            return false;
        });
    }

    private static loadArchives(template: Template) {
        $.getJSON('/Blog.svc/Archives?r=' + Math.random(),
            function (data) {
                $('.archives').append(template.render(data));
                $('.archives').change(function () {
                    var segments = $(this).val().split(',');
                    $('.stories').val('/');
                    $.getJSON('/Blog.svc/Month/' + segments[0] + '/' + segments[1] + '?r=' + Math.random(),
                        function (data) { renderPosts(data); });
                });
            });
    }

    private static loadStories(template: Template) {
        $.getJSON('/Blog.svc/Stories?r=' + Math.random(),
            function (data) {
                $('.stories').append(template.render(data));
                $('.stories').change(function () {
                    $('.archives').val('/');
                    loadPost($(this).val());
                });
            });
    }

    private static renderEditor(postId: number, post) {
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

                $('.posts').html(editorTemplate.render(post));

                if (postId) {
                    post.isEditorMode = true;
                    $('.posts').append(commentsTemplate.render(post));
                }

                $('.viewType').change(function () { changeViewType(); });
                $('.addAssetLink').click(function () {
                    $('.addAssetLink').hide();
                    $('.addAsset').show();
                    return false;
                });
                $('.hideAddAsset').click(function () {
                    $('.addAsset').hide();
                    $('.addAssetLink').show();
                    return false;
                });
                $('.submitPost').click(function () {
                    addOrEditPost(postId);
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
                            data: JSON.stringify({ request: { Credentials: userCredentials, Id: postId } }),
                            dataType: "json",
                            success: function () { document.location.reload(true); }
                        });
                    }
                    return false;
                });

                setTimeout(function () { saveEditorState(); }, 5000);
            });
    }

    private static saveEditorState() {
        if ($('.editor').length == 0)
            return;

        $('.editorState').html('Saving...');

        var postRequest = {
            post: new Post($('.title').text(), $('.content').html(), 'Normal')
        };

        if (editorViewType == 'html')
            postRequest.post.Content = $('.content').text();
        if ($('.isStory').attr('checked'))
            postRequest.post.Type = 'Story';

        postJson('/Blog.svc/SaveEditorState', postRequest,
            function () { setTimeout(function () { $('.editorState').html('Saved'); }, 500); });
        setTimeout(function () { saveEditorState(); }, 5000);
    }

    private static changeViewType() {
        if (editorViewType == 'normal') {
            editorViewType = 'html';
            $('.content').text($('.content').html());
        }
        else {
            editorViewType = 'normal';
            $('.content').html($('.content').text());
        }
    }

    private static addOrEditPost(postId: number) {
        var postRequest = {
            request: {
                Credentials: userCredentials,
                Post: new Post($('.title').text(), $('.content').html(), 'Normal')
            }
        };

        if (editorViewType == 'html')
            postRequest.request.Post.Content = $('.content').text();
        if ($('.isStory').attr('checked'))
            postRequest.request.Post.Type = 'Story';
        if (postId)
            postRequest.request.Post.Id = postId;

        postJson('/Blog.svc/AddOrEditPost', postRequest, function () { document.location.reload(true); });
    }

    private static submitComment(postId: number) {
        $('.commentAuthor').attr('disabled', 'disabled');
        $('.commentText').attr('disabled', 'disabled');
        $('.commentSubmitButton').attr('disabled', 'disabled');

        var newComment = {
            comment: {
                Author: $('.commentAuthor').val(),
                Text: $('.commentText').val()
            }
        };

        postJson('/Blog.svc/AddComment/' + postId, newComment, function () { loadPost(postId); });
    }

    public static Run() {

        editorViewType = 'normal';

        downloadTemplateAsync('post', function (template) {
            postTemplate = template;
            var id = getParameterByName('id')
            if (id != null)
                loadPost(parseInt(id));
            else {
                $.ajax({
                    method: 'GET',
                    async: false,
                    url: '/Blog.svc/LatestPosts?r=' + Math.random(),
                    success: function (data) { renderPosts(data); }
                });
            }
        });

        downloadTemplate('comments', function (template) { commentsTemplate = template; });
        downloadTemplate('authentication', function (template) {
            authenticationTemplate = template;

            $('.newPostLink').click(function () {
                showAuthenticationZone(this);
                return false;
            });
        });
        downloadTemplate('editor', function (template) { editorTemplate = template; });

        downloadTemplate('archive', function (template) { loadArchives(template); });
        downloadTemplate('story', function (template) { loadStories(template); });

        downloadTemplateAsync('quote', function (template) {
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
            loadLatest();
        });

        $('.searchButton').click(function () {
            $.getJSON('/Blog.svc/Search/' + $('.searchTerm').val() + '?r=' + Math.random(),
                function (data) { renderPosts(data); });
        });
    }
}

$().ready(function () { MainBlogControl.Run(); }); 