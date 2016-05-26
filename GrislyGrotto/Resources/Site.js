var Credentials = (function () {
    function Credentials(username, password) {
        this.Username = username;
        this.Password = password;
    }
    return Credentials;
})();
var Post = (function () {
    function Post(title, content, type) {
        this.Title = title;
        this.Content = content;
        this.Type = type;
    }
    return Post;
})();
var MainBlogControl = (function () {
    function MainBlogControl() { }
    MainBlogControl.getParameterByName = function getParameterByName(name) {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    };
    MainBlogControl.downloadTemplateAsync = function downloadTemplateAsync(title, onDownloaded) {
        $.ajax({
            method: 'GET',
            async: false,
            url: '/resources/templates/' + title + '.template.htm?v=10.4',
            success: function (data) {
                onDownloaded($.templates(data));
            }
        });
    };
    MainBlogControl.downloadTemplate = function downloadTemplate(title, onDownloaded) {
        $.get('/resources/templates/' + title + '.template.htm?v=10.4', function (data) {
            onDownloaded($.templates(data));
        });
    };
    MainBlogControl.postJson = function postJson(url, data, onSuccess) {
        $.ajax({
            type: "POST",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            dataType: "json",
            success: function (result) {
                onSuccess(result);
            }
        });
    };
    MainBlogControl.showAuthenticationZone = function showAuthenticationZone(trigger, postId) {
        $('.authenticationZone').remove();
        $(trigger).after(MainBlogControl.authenticationTemplate.render());
        $(trigger).hide();
        $('.checkedForValidCredentials').keyup(function () {
            if($('.username').val() != '' && $('.password').val() != '') {
                $('.authenticate').removeAttr('disabled');
            } else {
                $('.authenticate').attr('disabled', 'disabled');
            }
        });
        $('.authenticate').click(function () {
            MainBlogControl.checkAuthentication(postId);
        });
    };
    MainBlogControl.checkAuthentication = function checkAuthentication(postId) {
        MainBlogControl.userCredentials = new Credentials($('.username').val(), $('.password').val());
        var methodUrl = '/Blog.svc/';
        if(postId) {
            methodUrl += 'CheckAuthenticationForPost/' + postId;
        } else {
            methodUrl += 'CheckAuthentication';
        }
        MainBlogControl.postJson(methodUrl, MainBlogControl.userCredentials, function (result) {
            $('.authenticationZone').remove();
            if(!result) {
                $('.authenticationError').text('Authentication failed, please try again');
                return;
            }
            if(postId) {
                $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(), function (data) {
                    MainBlogControl.renderEditor(postId, data);
                });
            } else {
                MainBlogControl.renderEditor(postId, {
                    Title: '',
                    Content: ''
                });
            }
        });
    };
    MainBlogControl.loadLatest = function loadLatest() {
        $.getJSON('/Blog.svc/LatestPosts?r=' + Math.random(), function (data) {
            MainBlogControl.renderPosts(data);
        });
    };
    MainBlogControl.loadPost = function loadPost(postId, scrollToComments) {
        $.getJSON('/Blog.svc/Post/' + postId + '?r=' + Math.random(), function (data) {
            MainBlogControl.renderPosts(data);
            $('.posts').append(MainBlogControl.commentsTemplate.render(data));
            $('.commentSubmitButton').click(function () {
                MainBlogControl.submitComment(parseInt($(this).attr('name')));
            });
            $('.checkedForValidComment').keyup(function () {
                if($('.commentAuthor').val() != '' && $('.commentText').val() != '') {
                    $('.commentSubmitButton').removeAttr('disabled');
                } else {
                    $('.commentSubmitButton').attr('disabled', 'disabled');
                }
            });
            $('.editPostLink').click(function () {
                MainBlogControl.showAuthenticationZone(this, postId);
                return false;
            });
            if(scrollToComments) {
                document.getElementById('commentsBegin').scrollIntoView(true);
            }
        });
    };
    MainBlogControl.renderPosts = function renderPosts(data) {
        $('.posts').html(MainBlogControl.postTemplate.render(data));
        $('.postLink').click(function () {
            MainBlogControl.loadPost(parseInt($(this).attr('name')), true);
            return false;
        });
    };
    MainBlogControl.loadArchives = function loadArchives(template) {
        $.getJSON('/Blog.svc/Archives?r=' + Math.random(), function (data) {
            $('.archives').append(template.render(data));
            $('.archives').change(function () {
                var segments = $(this).val().split(',');
                $('.stories').val('/');
                $.getJSON('/Blog.svc/Month/' + segments[0] + '/' + segments[1] + '?r=' + Math.random(), function (data) {
                    MainBlogControl.renderPosts(data);
                });
            });
        });
    };
    MainBlogControl.loadStories = function loadStories(template) {
        $.getJSON('/Blog.svc/Stories?r=' + Math.random(), function (data) {
            $('.stories').append(template.render(data));
            $('.stories').change(function () {
                $('.archives').val('/');
                MainBlogControl.loadPost($(this).val());
            });
        });
    };
    MainBlogControl.renderEditor = function renderEditor(postId, post) {
        $.getJSON('/Blog.svc/GetEditorState?r=' + Math.random(), function (data) {
            if(data != null) {
                if(!post) {
                    post = data;
                } else {
                    post.Title = data.Title;
                    post.Content = data.Content;
                }
            }
            $('.posts').html(MainBlogControl.editorTemplate.render(post));
            if(postId) {
                post.isEditorMode = true;
                $('.posts').append(MainBlogControl.commentsTemplate.render(post));
            }
            $('.viewType').change(function () {
                MainBlogControl.changeViewType();
            });
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
                MainBlogControl.addOrEditPost(postId);
                return false;
            });
            $('.deleteCommentLink').click(function () {
                if(confirm('Are you sure you want to delete this comment?')) {
                    var trigger = $(this);
                    $.post('/Blog.svc/DeleteComment/' + $(this).attr('href'), function () {
                        $(trigger).parent().remove();
                    });
                }
                return false;
            });
            $('.deletePostLink').click(function () {
                if(confirm('Are you sure you want to delete this post?')) {
                    $.ajax({
                        type: "POST",
                        url: '/Blog.svc/DeletePost',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            request: {
                                Credentials: MainBlogControl.userCredentials,
                                Id: postId
                            }
                        }),
                        dataType: "json",
                        success: function () {
                            document.location.reload(true);
                        }
                    });
                }
                return false;
            });
            setTimeout(function () {
                MainBlogControl.saveEditorState();
            }, 5000);
        });
    };
    MainBlogControl.saveEditorState = function saveEditorState() {
        if($('.editor').length == 0) {
            return;
        }
        $('.editorState').html('Saving...');
        var postRequest = {
            post: new Post($('.title').text(), $('.content').html(), 'Normal')
        };
        if(MainBlogControl.editorViewType == 'html') {
            postRequest.post.Content = $('.content').text();
        }
        if($('.isStory').attr('checked')) {
            postRequest.post.Type = 'Story';
        }
        MainBlogControl.postJson('/Blog.svc/SaveEditorState', postRequest, function () {
            setTimeout(function () {
                $('.editorState').html('Saved');
            }, 500);
        });
        setTimeout(function () {
            MainBlogControl.saveEditorState();
        }, 5000);
    };
    MainBlogControl.changeViewType = function changeViewType() {
        if(MainBlogControl.editorViewType == 'normal') {
            MainBlogControl.editorViewType = 'html';
            $('.content').text($('.content').html());
        } else {
            MainBlogControl.editorViewType = 'normal';
            $('.content').html($('.content').text());
        }
    };
    MainBlogControl.addOrEditPost = function addOrEditPost(postId) {
        var postRequest = {
            request: {
                Credentials: MainBlogControl.userCredentials,
                Post: new Post($('.title').text(), $('.content').html(), 'Normal')
            }
        };
        if(MainBlogControl.editorViewType == 'html') {
            postRequest.request.Post.Content = $('.content').text();
        }
        if($('.isStory').attr('checked')) {
            postRequest.request.Post.Type = 'Story';
        }
        if(postId) {
            postRequest.request.Post.Id = postId;
        }
        MainBlogControl.postJson('/Blog.svc/AddOrEditPost', postRequest, function () {
            document.location.reload(true);
        });
    };
    MainBlogControl.submitComment = function submitComment(postId) {
        $('.commentAuthor').attr('disabled', 'disabled');
        $('.commentText').attr('disabled', 'disabled');
        $('.commentSubmitButton').attr('disabled', 'disabled');
        var newComment = {
            comment: {
                Author: $('.commentAuthor').val(),
                Text: $('.commentText').val()
            }
        };
        MainBlogControl.postJson('/Blog.svc/AddComment/' + postId, newComment, function () {
            MainBlogControl.loadPost(postId);
        });
    };
    MainBlogControl.Run = function Run() {
        MainBlogControl.editorViewType = 'normal';
        MainBlogControl.downloadTemplateAsync('post', function (template) {
            MainBlogControl.postTemplate = template;
            var id = MainBlogControl.getParameterByName('id');
            if(id != null) {
                MainBlogControl.loadPost(parseInt(id));
            } else {
                $.ajax({
                    method: 'GET',
                    async: false,
                    url: '/Blog.svc/LatestPosts?r=' + Math.random(),
                    success: function (data) {
                        MainBlogControl.renderPosts(data);
                    }
                });
            }
        });
        MainBlogControl.downloadTemplate('comments', function (template) {
            MainBlogControl.commentsTemplate = template;
        });
        MainBlogControl.downloadTemplate('authentication', function (template) {
            MainBlogControl.authenticationTemplate = template;
            $('.newPostLink').click(function () {
                MainBlogControl.showAuthenticationZone(this);
                return false;
            });
        });
        MainBlogControl.downloadTemplate('editor', function (template) {
            MainBlogControl.editorTemplate = template;
        });
        MainBlogControl.downloadTemplate('archive', function (template) {
            MainBlogControl.loadArchives(template);
        });
        MainBlogControl.downloadTemplate('story', function (template) {
            MainBlogControl.loadStories(template);
        });
        MainBlogControl.downloadTemplateAsync('quote', function (template) {
            $.ajax({
                method: 'GET',
                async: false,
                url: '/Blog.svc/Quote?r=' + Math.random(),
                success: function (data) {
                    $('.quote').html(template.render(data));
                }
            });
        });
        $('.header').click(function () {
            $('.archives').val('/');
            $('.stories').val('/');
            $('.newPostLink').show();
            MainBlogControl.loadLatest();
        });
        $('.searchButton').click(function () {
            $.getJSON('/Blog.svc/Search/' + $('.searchTerm').val() + '?r=' + Math.random(), function (data) {
                MainBlogControl.renderPosts(data);
            });
        });
    };
    return MainBlogControl;
})();
$().ready(function () {
    MainBlogControl.Run();
});
