using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;

namespace DAL.SQLite
{
    public class PostServices
    {
        private readonly DataAccess dataAccess;
        private readonly IUserService userService;

        public PostService(ConnectionInfo connectionInfo, IUserService usableUserService)
        {
            dataAccess = new DataAccess(connectionInfo);
            userService = usableUserService;
        }

        private Post PostFrom(DataRow row)
        {
            return new Post
             {
                 ID = (int)row["PostID"],
                 User = userService.GetUserByFullName(row["User"].ToString()),
                 Title = row["Title"].ToString(),
                 Created = DateTime.Parse(row["Created"].ToString()),
                 Status = (PostStatus)Enum.Parse(typeof(PostStatus), row["Status"].ToString()),
                 Content = row["Content"].ToString(),
                 Comments = CommentsOfPost((int)row["PostID"])
             };
        }

        private IEnumerable<Comment> CommentsOfPost(int postID)
        {
            const string query = "SELECT * FROM Comments WHERE PostID = @postID ORDER BY Created";
            var results = dataAccess.RetrieveDataSet(query, new SQLiteParameter("@postID", postID));
            if (results.Tables.Count == 0)
                yield break;
            foreach (DataRow row in results.Tables[0].Rows)
                yield return CommentFrom(row);
        }

        private Comment CommentFrom(DataRow row)
        {
            return new Comment
           {
               Author = row["Author"].ToString(),
               Created = DateTime.Parse(row["Created"].ToString()),
               Content = row["Text"].ToString()
           };
        }

        public IEnumerable<Post> GetLatest(User user, DateTime fromTime, int? count, PostStatus type)
        {
            var query = "SELECT * FROM Posts WHERE ";
            if (user != null) query += "User = @user AND ";
            query += "Created < @fromTime AND Status = @status ORDER BY Created DESC";
            if (count.HasValue) query += " LIMIT @count";

            var results = dataAccess.RetrieveDataSet(query,
                new SQLiteParameter("@user", user != null ? user.FullName : string.Empty),
                new SQLiteParameter("@fromTime", fromTime),
                new SQLiteParameter("@status", type.ToString()),
                new SQLiteParameter("@count", count.HasValue ? count.Value : default(int)));

            foreach (DataRow row in results.Tables[0].Rows)
                yield return PostFrom(row);
        }

        public Post GetSpecific(int postID)
        {
            const string query = "SELECT * FROM Posts WHERE PostID = @ID";
            var results = dataAccess.RetrieveDataSet(query, new SQLiteParameter("@ID", postID));
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
                return PostFrom(results.Tables[0].Rows[0]);
            return null;
        }

        public IEnumerable<Post> GetFromMonth(User user, int year, string monthName, PostStatus type)
        {
            var startTime = DateTime.Parse(string.Format("1 {0} {1}", monthName, year));

            var query = "SELECT * FROM Posts WHERE ";
            if (user != null) query += "User = @user AND ";
            query += "Created >= @startTime AND Created < @endTime AND Status = @status ORDER BY Created DESC";

            var results = dataAccess.RetrieveDataSet(query,
                new SQLiteParameter("@user", user != null ? user.FullName : string.Empty),
                new SQLiteParameter("@status", type.ToString()),
                new SQLiteParameter("@startTime", startTime),
                new SQLiteParameter("@endTime", startTime.AddMonths(1)));

            foreach (DataRow row in results.Tables[0].Rows)
                yield return PostFrom(row);
        }

        public Dictionary<DateTime, int> GetMonthPostCounts(User user)
        {
            var query = "SELECT Created FROM Posts";
            if (user != null) query += " WHERE User = @user";
            query += " ORDER BY Created DESC";

            var results = dataAccess.RetrieveDataSet(query, new SQLiteParameter("@user", user != null ? user.FullName : string.Empty));
            var monthCounts = new Dictionary<DateTime, int>();

            foreach (DataRow row in results.Tables[0].Rows)
            {
                var created = DateTime.Parse(row["Created"].ToString());
                var key = new DateTime(created.Year, created.Month, 1);
                if (monthCounts.ContainsKey(key))
                    monthCounts[key]++;
                else
                    monthCounts.Add(key, 1);
            }

            return monthCounts;
        }

        public IEnumerable<Post> Search(User user, string searchTerm, PostStatus type)
        {
            var query = "SELECT * FROM Posts WHERE ";
            if (user != null) query += "User = @user AND ";
            query += "(Title LIKE @searchTerm OR Content LIKE @searchTerm) AND Status = @status ORDER BY Created DESC";

            var results = dataAccess.RetrieveDataSet(query,
                new SQLiteParameter("@user", user != null ? user.FullName : string.Empty),
                new SQLiteParameter("@status", type.ToString()),
                new SQLiteParameter("@searchTerm", "%" + searchTerm + "%"));

            foreach (DataRow row in results.Tables[0].Rows)
                yield return PostFrom(row);
        }

        public Post AddPost(Post post)
        {
            post.ID = (int)dataAccess.RetrieveDataSet("SELECT PostID FROM Posts ORDER BY PostID DESC LIMIT 1").Tables[0].Rows[0].ItemArray[0] + 1;
            dataAccess.ExecuteNonQuery("INSERT INTO Posts VALUES (@postID, @author, @entryDate, @title, @status, @content)",
                new SQLiteParameter("@postID", post.ID),
                new SQLiteParameter("@author", post.User.FullName),
                new SQLiteParameter("@entryDate", post.Created),
                new SQLiteParameter("@title", post.Title),
                new SQLiteParameter("@status", post.Status.ToString()),
                new SQLiteParameter("@content", post.Content));

            return post;
        }

        public void UpdatePost(Post post)
        {
            dataAccess.ExecuteNonQuery("UPDATE Posts SET Title = @title, Status = @status, Content = @content WHERE PostID = @ID",
                new SQLiteParameter("@title", post.Title),
                new SQLiteParameter("@status", post.Status.ToString()),
                new SQLiteParameter("@content", post.Content),
                new SQLiteParameter("@ID", post.ID));
        }

        public void DeletePost(int postID)
        {
            dataAccess.ExecuteNonQuery("DELETE FROM Posts WHERE PostID = @ID", new SQLiteParameter("@ID", postID));
        }

        public void AddComment(Post post, Comment comment)
        {
            dataAccess.ExecuteNonQuery("INSERT INTO Comments VALUES (@PostID, @Author, @Created, @Text)",
                new SQLiteParameter("@PostID", post.ID),
                new SQLiteParameter("@Author", comment.Author),
                new SQLiteParameter("@Created", comment.Created),
                new SQLiteParameter("@Text", comment.Content));
        }
    }
}