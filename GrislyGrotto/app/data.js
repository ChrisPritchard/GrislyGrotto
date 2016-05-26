
var util = require('./util');
var sql = require('node-sqlserver-unofficial');

var connectionString = 'Driver={SQL Server Native Client 11.0};Server=tcp:mhvrgafbmv.database.windows.net,1433;Database=GrislyGrottoDB_v12.8;Uid=grislygrotto_dbuser@mhvrgafbmv;Pwd=***REMOVED***;Encrypt=yes;Connection Timeout=30;';

exports.latest = function(callback) {
    sql.query(connectionString, 'SELECT TOP 5 *, CommentCount = (SELECT Count(*) FROM Comments WHERE Comments.Post_ID = Posts.ID) FROM Posts INNER JOIN Users ON Posts.Author_ID = Users.ID ORDER BY [Date] DESC;',
        function (err, results) {
            callback(err, results);
        });
};

exports.single = function (id, callback) {
    sql.query(connectionString, 'SELECT TOP 1 * FROM Posts INNER JOIN Users ON Posts.Author_ID = Users.ID WHERE [Key] = ?', [id],
        function (err, results) {
            if (results.length == 0 || err)
                callback(err, null);
            var post = results[0];
            sql.query(connectionString, 'SELECT * FROM Comments WHERE Post_ID = ? ORDER BY [Date]', [post.ID], function (err2, results2) {
                post.Comments = results2;
                callback(err, post);
            });
        });
};

exports.stories = function (callback) {
    sql.query(connectionString, 'SELECT [Key], Title, DisplayName, [Date], WordCount FROM Posts INNER JOIN Users ON Posts.Author_ID = Users.ID WHERE IsStory = 1 ORDER BY [Date] DESC',
        function (err, results) {
            callback(err, results);
        });
};

exports.archives = function (callback) {
    sql.query(connectionString, 'SELECT *, [Count] = Count(*) FROM (SELECT [Year] = DATEPART(yyyy, [Date]), [Month] = DATEPART(mm, [Date]) FROM Posts) AS Months GROUP BY [Year], [Month] ORDER BY [Year] DESC, [Month] DESC',
        function (err, results) {
            if (err)
                callback(err, null);
            var years = [];
            for (var i in results)
                if (i == 0 || years[years.length - 1].name != results[i].Year.toString())
                    years[years.length] = { name: results[i].Year, months: [{ name: util.months[results[i].Month - 1], count: results[i].Count }] };
                else
                    years[years.length - 1].months.push({ name: util.months[results[i].Month - 1], count: results[i].Count });
            callback(err, years);
        });
};

exports.month = function (monthname, year, callback) {
    sql.query(connectionString, 'SELECT [Key], Title, DisplayName, [Date] FROM Posts INNER JOIN Users ON Posts.Author_ID = Users.ID WHERE DATEPART(yyyy, [Date]) = ? AND DATEPART(mm, [Date]) = ? ORDER BY [Date] DESC',
        [year, util.months.indexOf(monthname) + 1],
        function (err, results) {
            callback(err, results);
        });
};

exports.search = function (searchTerm, callback) {
    var searchTermParam = '%' + searchTerm + '%';
    sql.query(connectionString, 'SELECT [Key], Title, DisplayName, [Date] FROM Posts INNER JOIN Users ON Posts.Author_ID = Users.ID WHERE Title LIKE ? OR Content LIKE ? ORDER BY [Date] DESC', [searchTermParam, searchTermParam],
        function (err, results) {
            callback(err, results);
        });
}

exports.existsForId = function(id, callback) {
    sql.query(connectionString, 'SELECT TOP 1 Title FROM POSTS WHERE [Key] = ?', [id],
    function (err, results) {
        callback(err || results.length > 0);
    });
};

exports.newComment = function (comment, callback) {
    sql.query(connectionString, 'INSERT INTO Comments (Post_ID, Author, Content, Date) VALUES (?, ?, ?, ?)', [comment.Post_ID, comment.Author, comment.Content, util.getNzDate()],
        function (err, results) {
            callback(err, results);
        });
};

exports.validateUser = function (username, password, callback) {
    sql.query(connectionString, 'SELECT TOP 1 ID, DisplayName, Username, ImageUrl FROM Users WHERE Username = ? AND Password = ?', [username, password],
        function (err, results) {
            callback(err, results[0]);
        });
};

exports.create = function (post, callback) {
    sql.query(connectionString, 'INSERT INTO Posts (Title, [Key], Author_ID, Content, WordCount, IsStory, [Date]) Values (?, ?, ?, ?, ?, ?, ?)',
        [post.Title, post.Key, post.Author_ID, post.Content, post.WordCount, post.IsStory == 'on' ? 1 : 0, util.getNzDate()],
        function (err, results) {
            callback(err, results);
        });
};

exports.update = function(id, post, callback) {
    sql.query(connectionString, 'UPDATE Posts SET Title = ?, [Key] = ?, Content = ?, WordCount = ?, IsStory = ? WHERE ID = ?',
        [post.Title, post.Key, post.Content, post.WordCount, post.IsStory == 'on' ? 1 : 0, id],
        function (err, results) {
            callback(err, results);
        });
};