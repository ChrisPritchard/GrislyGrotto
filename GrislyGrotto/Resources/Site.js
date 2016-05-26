var id = getParameterByName('id');
var year = getParameterByName('year');
var month = getParameterByName('month');
var searchTerm = getParameterByName('searchTerm');
var editor = getParameterByName('editor');
var viewType = 'normal';

var postTemplate = '<div class="post"><h2 class="title">[Title]</h2><span class="details">[Author] - [TimePosted]</span><br/>[Link]<div class="content">[Content]</div></div>';
var editorElements = '<input type="radio" name="ViewType" class="viewType" checked="true"></input>Normal<input type="radio" name="ViewType" class="viewType"></input>HTML    <input type="checkbox" class="isStory"></input>Is Story<br/>'
            + '<button class="submitButton">Submit</button>';
var editorPostTemplate = '<div class="post editor"><h2 class="title" contenteditable="true">[Title]</h2><span class="details">[Author] - [TimePosted]</span><div class="content" contenteditable="true">[Content]</div></div>'
            + editorElements;
var newPostTemplate = '<div class="post editor"><h2 class="title" contenteditable="true">Enter Title Here</h2><div class="content" contenteditable="true">Enter Content Here</div></div>'
            + editorElements;
var optionTemplate = '<option value="[Value]" [Selected]>[Text]</option>';
var commentLinkTemplate = '<a class="postLink" href="#">Comments ([CommentsCount])</a>';
var commentTemplate = '<div class="comment"><h3>[Author] - [TimeMade]</h3><p>[Content]</p></div>';
var commentEditorTemplate = '<div class="commentEditor">Author:<input type="text" class="commentAuthor" /><br /><textarea class="commentContent"></textarea><br /><button class="commentSubmitButton">Submit</button></div>';

var username;
var password;
var currentAuthor;

$().ready(function ()
{
    $('.header').click(function ()
    {
        $('.archives').val('/');
        $('.stories').val('/');
        loadLatest();
    });

    $.getJSON('/Posts/Quote', function (data)
    {
        $('.quote').html('<span class="quoteContent">' + data.Content + '</span><span class="quoteAuthor">' + data.Author + '</span>');
    });

    $('.searchButton').click(function ()
    {
        loadSearchResults($('.searchTerm').val());
    });

    $('.loginLink').click(function ()
    {
        $(this).hide();
        $('.loginPanel').show();
        $('.username').focus();
        return false;
    });

    $('.loginButton').click(function () { authenticateUser(); });

    addCentralPostZone();

    loadArchives();
    loadStories();
});

function getParameterByName(name)
{
    var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
    return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
}

function authenticateUser()
{
    username = $('.username').val();
    password = $('.password').val();

    $.getJSON('/Posts/Authenticate/' + id,
        { username: username, password: password },
        function (data)
        {
            if (data != null)
            {
                currentAuthor = data;
                $('.loginPanel').after('<a href="#" class="newPostLink">Add new post</a>');
                $('.newPostLink').click(function ()
                {
                    loadNewPostEditor();
                    return false;
                });
                $('.loginPanel').hide();

                addCentralPostZone();
            }
            else
            {
                $('.error').remove();
                $('.loginButton').after('<br /><span class="error">Username and/or Password is incorrect</span>');
            }
        });
}

function addCentralPostZone()
{
    if (id != null)
        loadPost(id);
    else if (year != null && month != null)
        loadMonth(year, month);
    else if (searchTerm != null)
        loadSearchResults(searchTerm);
    else if (editor != null)
        loadNewPostEditor();
    else
        loadLatest();
}

function loadPost(postid)
{
    id = postid;
    month = null;
    year = null;
    searchTerm = null;

    $.getJSON('/Posts/' + postid, function (data)
    {
        var post = postTemplate;
        if (editor != null)
            post = editorPostTemplate;

        post = post.replace('[Title]', data.Title)
            .replace('[Author]', data.Author)
            .replace('[TimePosted]', data.TimePostedWeb)
            .replace('[Content]', data.Content);

        if (data.IsStory)
            $('.isStory').attr('checked', 'checked');

        if (currentAuthor != null && data.Author == currentAuthor)
            post = post.replace('[Link]', '<a class="postLink" href="#">Edit</a>');
        else
            post = post.replace('[Link]', '');
        $('.posts').html(post);
        $('.postLink').click(function ()
        {
            editPost(data.Id, data.IsStory);
            return false;
        });
        $('.viewType').change(function () { changeViewType(); });

        if (editor == null)
        {
            if (data.Comments.length > 0)
            {
                $('.posts').append('<h2 class="commentsHeading">Comments</h2><div class="comments"></div>');
                for (var i = 0; i < data.Comments.length; i++)
                {
                    var comment = commentTemplate
                .replace('[Author]', data.Comments[i].Author)
                .replace('[TimeMade]', data.Comments[i].TimeMadeWeb)
                .replace('[Content]', data.Comments[i].Content)
                    $('.comments').append(comment);
                }
            }
            $('.posts').append(commentEditorTemplate);
            $('.commentSubmitButton').click(function () { addComment(data.Id); });
        }
    });
}

function loadMonth(postyear, postmonth)
{
    id = null;
    month = postmonth;
    year = postyear;
    searchTerm = null;

    $.getJSON('/Posts/' + postyear + '/' + postmonth,
        function (data) { renderPosts(data); });
}

function loadSearchResults(postsearchTerm)
{
    id = null;
    month = null;
    year = null;
    searchTerm = postsearchTerm;

    $.getJSON('/Posts/Search',
    { searchTerm: postsearchTerm },
        function (data) { renderPosts(data); });
}

function loadLatest()
{
    id = null;
    month = null;
    year = null;
    searchTerm = null;

    $.getJSON('/Posts', 
        function (data) { renderPosts(data); });
}

function loadNewPostEditor()
{
    $('.posts').html(newPostTemplate);
    $('.viewType').change(function () { changeViewType(); });
    $('.submitButton').click(function () { addPost(); });
}

function renderPosts(data)
{
    $('.posts').html('');
    for (i = 0; i < data.length; i++)
    {
        var post = postTemplate
                .replace('[Title]', data[i].Title)
                .replace('[Author]', data[i].Author)
                .replace('[TimePosted]', data[i].TimePostedWeb)
                .replace('[Link]', commentLinkTemplate)
                .replace('[CommentsCount]', data[i].Comments.length)
                .replace('[Content]', data[i].Content);
        $('.posts').append(post);
    }

    var links = $('.postLink');
    for (i = 0; i < data.length; i++)
    {
        $(links[i]).attr('name', data[i].Id);
        $(links[i]).click(function ()
        {
            loadPost($(this).attr('name'));
            return false;
        });
    }
}

function loadArchives()
{
    $.getJSON('/Posts/Archives', function (data)
    {
        for (i = 0; i < data.length; i++)
        {
            var value = data[i].Year + ',' + data[i].MonthName;
            var text = data[i].MonthName + ', ' + data[i].Year + ' (' + data[i].PostCount + ')';
            var option = optionTemplate
                .replace('[Value]', value)
                .replace('[Text]', text);
            if (year != null && month != null && year == data[i].Year && month == data[i].MonthName)
                option = option.replace('[Selected]', 'selected');
            else
                option = option.replace('[Selected]', '');
            $('.archives').append(option);
        }

        $('.archives').change(function ()
        {
            var segments = $(this).val().split(',');
            $('.stories').val('/');
            loadMonth(segments[0], segments[1]);
        });
    });
}

function loadStories()
{
    $.getJSON('/Posts/Stories', function (data)
    {
        for (i = 0; i < data.length; i++)
        {
            var value = data[i].Id;
            var text = data[i].Title + ' (' + data[i].Author + ', ' + data[i].WordCount + ' words)';
            var option = optionTemplate
                .replace('[Value]', value)
                .replace('[Text]', text);
            if (id != null && id == data[i].Id)
                option = option.replace('[Selected]', 'selected');
            else
                option = option.replace('[Selected]', '');
            $('.stories').append(option);
        }

        $('.stories').change(function ()
        {
            $('.archives').val('/');
            loadPost($(this).val());
        });
    });
}

function editPost(id, isstory)
{
    $('.postLink').remove();
    $('.title').attr('contenteditable', 'true');
    $('.content').attr('contenteditable', 'true');
    $('.post').after(editorElements);
    $('.viewType').change(function () { changeViewType(); });
    if (isstory)
        $('.isStory').attr('checked', 'checked');
    $('.submitButton').click(function () { updatePost(id); });
    $('.comment').remove();
    $('.commentEditor').remove();
}

function changeViewType()
{
    if (viewType == 'normal')
    {
        viewType = 'html';
        $('.content').text($('.content').html());
    }
    else
    {
        viewType = 'normal';
        $('.content').html($('.content').text());
    }
}

function updatePost(id)
{
    var title = $('.title').text();
    var content = $('.content').html();
    if (viewType == 'html')
        content = $('.content').text();
    var isstory = false;
    if($('.isStory').attr('checked'))
        isstory = true;

    $.getJSON('/Posts/Authenticate/' + id,
        { username: username, password: password },
        function (data)
        {
            if (data != null)
            {
                $.post('/Posts/' + id,
                    { title: title, content: content, username: username, password: password, isstory: isstory },
                    function ()
                    { loadLatest(); });
            }
            else
            {
                $('.error').remove();
                $('.submitButton').after('<br /><span class="error">Username and/or Password is incorrect</span>');
            }
        });
}

function addPost()
{
    var title = $('.title').text();
    var content = $('.content').html();
    if (viewType == 'html')
        content = $('.content').text();
    var isstory = false;
    if($('.isStory').attr('checked'))
        isstory = true;

    $.getJSON('/Posts/Authenticate',
        { username: username, password: password },
        function (data)
        {
            if (data != null)
            {
                $.post('/Posts',
                    { title: title, content: content, username: username, password: password, isstory: isstory },
                    function ()
                    { loadLatest(); });
            }
            else
            {
                $('.error').remove();
                $('.submitButton').after('<br /><span class="error">Username and/or Password is incorrect</span>');
            }
        });
}

function addComment(id)
{
    var author = $('.commentAuthor').val();
    var content = $('.commentContent').val();
    $('.commentSubmitButton').after('<p class="commentPostingStatus">Posting...</p>');

    $.post('/Posts/' + id + '/AddComment',
    { author: author, content: content },
    function ()
    {
        $('.commentAuthor').val('');
        $('.commentContent').val('');
        $('.commentPostingStatus').remove();

        var comment = commentTemplate
                .replace('[Author]', author)
                .replace('[TimeMade]', 'Just Added')
                .replace('[Content]', content)
        $('.comments').append(comment);
    });
}
