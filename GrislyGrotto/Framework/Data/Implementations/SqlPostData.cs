using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Data.Implementations
{
    public class SqlPostData : IPostData
    {
        private const int storySummaryWordCap = 300;

        public IEnumerable<Post> LatestPosts(int count, string user = null)
        {
            var query = "SELECT * FROM Posts ";
            if (user != null) query += "WHERE User = @user ";
            query += "ORDER BY Created DESC LIMIT @count";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", user ?? string.Empty),
                new SQLiteParameter("@count", count));

            return results.Tables[0].Rows.Cast<DataRow>().Select(r => AsPost(r));
        }

        public IEnumerable<Post> PostsByStatus(string status, string user = null)
        {
            var query = "SELECT * FROM Posts WHERE Status = @status ";
            if (user != null) query += "AND User = @user ";
            query += "ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", user ?? string.Empty),
                new SQLiteParameter("@status", status));

            return results.Tables[0].Rows.Cast<DataRow>().Select(r => AsPost(r, false));
        }

        public IEnumerable<Post> PostsForMonth(int year, int month, string user = null)
        {
            var startTime = DateTime.Parse(string.Format("1 {0} {1}", month.AsMonthName(), year));

            var query = "SELECT * FROM Posts WHERE ";
            if (user != null) query += "User = @user AND ";
            query += "Created >= @startTime AND Created < @endTime ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", user ?? string.Empty),
                new SQLiteParameter("@startTime", startTime),
                new SQLiteParameter("@endTime", startTime.AddMonths(1)));

            return results.Tables[0].Rows.Cast<DataRow>().Select(r => AsPost(r));
        }

        public IEnumerable<Post> SearchResults(string searchTerm)
        {
            const string query = "SELECT * FROM Posts WHERE (Title LIKE @searchTerm OR Content LIKE @searchTerm) ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@searchTerm", "%" + searchTerm + "%"));

            return results.Tables[0].Rows.Cast<DataRow>().Select(r => AsPost(r));
        }

        public IEnumerable<MonthCount> MonthPostCounts(string user = null)
        {
            var query = "SELECT Created FROM Posts";
            if (user != null) query += " WHERE User = @user";
            query += " ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@user", user ?? string.Empty));
            var monthCounts = new List<MonthCount>();

            foreach (var month in results.Tables[0].Rows.Cast<DataRow>()
                .Select(row => DateTime.Parse(row["Created"].ToString()))
                .Select(created => new MonthCount { Year = created.Year, Month = created.Month, PostCount = 1 }))
            {
                if (monthCounts.Any(m => m.Year == month.Year && m.Month == month.Month))
                    monthCounts[monthCounts.FindIndex(m => m.Year == month.Year && m.Month == month.Month)].PostCount++;
                else
                    monthCounts.Add(month);
            }

            return monthCounts;
        }

        public Post SinglePost(int id)
        {
            const string query = "SELECT * FROM Posts WHERE PostID = @ID";
            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@ID", id));
            return results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0
                ? AsPost(results.Tables[0].Rows[0], false) : null;
        }

        public int AddOrEditPost(Post post)
        {
            if (post.ID.HasValue)
            {
                DataAccess.Default.ExecuteNonQuery(
                    "UPDATE Posts SET Title = @title, Content = @content, Status = @status WHERE PostID = @ID",
                    new SQLiteParameter("@title", post.Title),
                    new SQLiteParameter("@content", post.Content),
                    new SQLiteParameter("@status", post.IsStory ? "Story" : "Post"),
                    new SQLiteParameter("@ID", post.ID.Value));
                return post.ID.Value;
            }

            post.ID = (int)DataAccess.Default.RetrieveDataSet("SELECT PostID FROM Posts ORDER BY PostID DESC LIMIT 1").Tables[0].Rows[0].ItemArray[0] + 1;
            DataAccess.Default.ExecuteNonQuery("INSERT INTO Posts VALUES (@postID, @author, @entryDate, @title, @status, @content)",
                new SQLiteParameter("@postID", post.ID),
                new SQLiteParameter("@author", post.Username),
                new SQLiteParameter("@entryDate", post.TimePosted),
                new SQLiteParameter("@title", post.Title),
                new SQLiteParameter("@status", post.IsStory ? "Story" : "Post"),
                new SQLiteParameter("@content", post.Content));

            return post.ID.Value;
        }

        public void AddComment(Comment comment, int postID)
        {
            DataAccess.Default.ExecuteNonQuery("INSERT INTO Comments VALUES (@PostID, @Author, @Created, @Text)",
                new SQLiteParameter("@PostID", postID),
                new SQLiteParameter("@Author", comment.Author),
                new SQLiteParameter("@Created", comment.TimeMade),
                new SQLiteParameter("@Text", comment.Content));
        }

        private static Post AsPost(DataRow row, bool summariseStories = true)
        {
            var post = new Post
            {
                ID = (int)row["PostID"],
                Username = row["User"].ToString(),
                Title = row["Title"].ToString(),
                TimePosted = DateTime.Parse(row["Created"].ToString()),
                Content = row["Content"].ToString().Trim(),
                Comments = CommentsOfPost((int)row["PostID"]).ToArray(),
                IsStory = row["Status"].ToString().Equals("Story")
            };
            if (summariseStories && post.IsStory && post.RawContent.Split(' ').Length > storySummaryWordCap)
                post.Content = "<p style=\"font-style:italic\">" + string.Concat(post.RawContent.Split(' ').Take(storySummaryWordCap).Select(w => w + " ").ToArray()) + "...</p>";
            return post;
        }

        private static IEnumerable<Comment> CommentsOfPost(int postID)
        {
            const string query = "SELECT * FROM Comments WHERE PostID = @postID ORDER BY Created";
            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@postID", postID));
            if (results.Tables.Count == 0)
                yield break;
            foreach (DataRow row in results.Tables[0].Rows)
                yield return CommentFrom(row);
        }

        private static Comment CommentFrom(DataRow row)
        {
            return new Comment
            {
                Author = row["Author"].ToString(),
                TimeMade = DateTime.Parse(row["Created"].ToString()),
                Content = row["Text"].ToString()
            };
        }
    }
}