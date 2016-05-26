using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using GrislyGrotto.Web.Core;

namespace GrislyGrotto.Web.Data
{
    public static class PostServices
    {
        private static Post PostFrom(DataRow row)
        {
            return new Post
            {
                ID = (int)row["PostID"],
                User = UserServices.GetUserByFullName(row["User"].ToString()),
                Title = row["Title"].ToString(),
                Created = DateTime.Parse(row["Created"].ToString()),
                Status = (PostStatus)Enum.Parse(typeof(PostStatus), row["Status"].ToString()),
                Content = row["Content"].ToString(),
                Comments = CommentsOfPost((int)row["PostID"])
             };
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
               Created = DateTime.Parse(row["Created"].ToString()),
               Content = row["Text"].ToString()
           };
        }

        public static IEnumerable<Post> GetLatest(int? count, PostStatus type, User userFilterOrNull)
        {
            var query = "SELECT * FROM Posts WHERE ";
            if (userFilterOrNull != null) query += "User = @user AND ";
            query += "Created < @fromTime AND Status = @status ORDER BY Created DESC";
            if (count.HasValue) query += " LIMIT @count";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", userFilterOrNull != null ? userFilterOrNull.FullName : string.Empty),
                new SQLiteParameter("@fromTime", DateTime.Now),
                new SQLiteParameter("@status", type.ToString()),
                new SQLiteParameter("@count", count.HasValue ? count.Value : default(int)));

            return results.Tables[0].Rows.Cast<DataRow>().Select(row => PostFrom(row));
        }

        public static Post GetSpecific(int postID)
        {
            const string query = "SELECT * FROM Posts WHERE PostID = @ID";
            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@ID", postID));
            return results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0
                ? PostFrom(results.Tables[0].Rows[0]) : null;
        }

        public static IEnumerable<Post> GetFromMonth(int year, string monthName, User userFilterOrNull)
        {
            var startTime = DateTime.Parse(string.Format("1 {0} {1}", monthName, year));

            var query = "SELECT * FROM Posts WHERE ";
            if (userFilterOrNull != null) query += "User = @user AND ";
            query += "Created >= @startTime AND Created < @endTime AND Status = @status ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", userFilterOrNull != null ? userFilterOrNull.FullName : string.Empty),
                new SQLiteParameter("@status", PostStatus.Published.ToString()),
                new SQLiteParameter("@startTime", startTime),
                new SQLiteParameter("@endTime", startTime.AddMonths(1)));

            return results.Tables[0].Rows.Cast<DataRow>().Select(row => PostFrom(row));
        }

        public static Dictionary<DateTime, int> GetMonthPostCounts(User userFilterOrNull)
        {
            var query = "SELECT Created FROM Posts";
            if (userFilterOrNull != null) query += " WHERE User = @user";
            query += " ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@user", userFilterOrNull != null ? userFilterOrNull.FullName : string.Empty));
            var monthCounts = new Dictionary<DateTime, int>();

            foreach (var key in results.Tables[0].Rows.Cast<DataRow>()
                .Select(row => DateTime.Parse(row["Created"].ToString()))
                .Select(created => new DateTime(created.Year, created.Month, 1)))
            {
                if (monthCounts.ContainsKey(key))
                    monthCounts[key]++;
                else
                    monthCounts.Add(key, 1);
            }

            return monthCounts;
        }

        public static IEnumerable<Post> Search(string searchTerm, int maxResults, User userFilterOrNull)
        {
            var query = "SELECT * FROM Posts WHERE ";
            if (userFilterOrNull != null) query += "User = @user AND ";
            query += "(Title LIKE @searchTerm OR Content LIKE @searchTerm) AND Status = @status ORDER BY Created DESC";

            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@user", userFilterOrNull != null ? userFilterOrNull.FullName : string.Empty),
                new SQLiteParameter("@status", PostStatus.Published.ToString()),
                new SQLiteParameter("@searchTerm", "%" + searchTerm + "%"));

            return results.Tables[0].Rows.Cast<DataRow>().Select(row => PostFrom(row)).Take(maxResults);
        }

        public static Post AddPost(Post post)
        {
            post.ID = (int)DataAccess.Default.RetrieveDataSet("SELECT PostID FROM Posts ORDER BY PostID DESC LIMIT 1").Tables[0].Rows[0].ItemArray[0] + 1;
            DataAccess.Default.ExecuteNonQuery("INSERT INTO Posts VALUES (@postID, @author, @entryDate, @title, @status, @content)",
                new SQLiteParameter("@postID", post.ID),
                new SQLiteParameter("@author", post.User.FullName),
                new SQLiteParameter("@entryDate", post.Created),
                new SQLiteParameter("@title", post.Title),
                new SQLiteParameter("@status", post.Status.ToString()),
                new SQLiteParameter("@content", post.Content));

            return post;
        }

        public static void UpdatePost(int postID, string title, string content)
        {
            DataAccess.Default.ExecuteNonQuery("UPDATE Posts SET Title = @title, Status = @status, Content = @content WHERE PostID = @ID",
                new SQLiteParameter("@title", title),
                new SQLiteParameter("@status", PostStatus.Published.ToString()),
                new SQLiteParameter("@content", content),
                new SQLiteParameter("@ID", postID));
        }

        public static void AddComment(int postID, Comment comment)
        {
            DataAccess.Default.ExecuteNonQuery("INSERT INTO Comments VALUES (@PostID, @Author, @Created, @Text)",
                new SQLiteParameter("@PostID", postID),
                new SQLiteParameter("@Author", comment.Author),
                new SQLiteParameter("@Created", comment.Created),
                new SQLiteParameter("@Text", comment.Content));
        }
    }
}