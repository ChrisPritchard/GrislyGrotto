using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using GrislyGrotto;

namespace DataMigrator
{
    static class Program
    {
        static void Main(string[] args)
        {
            var posts = DataAccess.Default.RetrieveDataSet("SELECT * FROM Posts");
            var comments = DataAccess.Default.RetrieveDataSet("SELECT * FROM Comments");

            var connectionString =
                string.Format(
                    "metadata=res://*/GrislyGrottoEntities.csdl|res://*/GrislyGrottoEntities.ssdl|res://*/GrislyGrottoEntities.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='Data Source={0}'",
                    Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "GrislyGrotto.sdf"));
            var entities = new GrislyGrottoEntities(connectionString);

            var oldIds = new Dictionary<int, DateTime>();
            foreach (DataRow row in posts.Tables[0].Rows)
            {
                var date = DateTime.Parse(row[2].ToString());
                entities.Posts.AddObject(new Post
                {
                    Title = row[3].ToString(),
                    Author = row[1].ToString(),
                    Created = date,
                    Type = row[4].ToString().Equals("Story") ? "Story" : "Post",
                    Content = row[5].ToString()
                });
                oldIds.Add(int.Parse(row[0].ToString()), date);
            }
            entities.SaveChanges();

            foreach (DataRow row in comments.Tables[0].Rows)
            {
                var postId = int.Parse(row[0].ToString());
                var oldDate = oldIds[postId];
                var newPosts = entities.Posts.Where(p => p.Created.Equals(oldDate)).ToArray();
                var newPostId = newPosts.Last().ID;
                entities.Comments.AddObject(new Comment
                {
                    PostID = newPostId,
                    Author = row[1].ToString(),
                    Created = DateTime.Parse(row[2].ToString()),
                    Text = row[3].ToString()
                });
            }
            entities.SaveChanges();
        }
    }
}
