using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMigrator.SqlCeToSqlSvr
{
    class Program
    {
        const string userContent = @"C:\Websites\GrislyGrotto\UserContent\";

        static void Main(string[] args)
        {
            //var entitiesAzure = new GrislyGrottoAzureEntities();
            //MigratePostsAndComments(entitiesAzure);

            //var allUserContent = Directory.GetFiles(userContent, "*.*", SearchOption.AllDirectories)
            //    .Select(p => new { Key = p.Substring(userContent.Length), Data = File.ReadAllBytes(p) }).ToArray();
            //foreach (var file in allUserContent)
            //    entitiesAzure.Assets.Add(new Asset { Key = file.Key, Data = file.Data });
            //entitiesAzure.SaveChanges();

            //var keys = entitiesAzure.Assets.Select(a => a.Key).ToArray();
        }

        private static void MigratePostsAndComments(GrislyGrottoAzureEntities entitiesAzure)
        {
            var entitiesCe = new CE.GrislyGrottoCeEntities();

            var existingComments = entitiesCe.CEComments.GroupBy(c => c.PostID).ToDictionary(c => c.Key, c => c.ToArray());
            var existingPosts = entitiesCe.CEPosts.ToArray();

            var postsWithComments = existingPosts.OrderBy(p => p.Created)
                .Select(p => new { Post = p, Comments = existingComments.ContainsKey(p.ID) ? existingComments[p.ID] : null }).ToArray();

            foreach (var pair in postsWithComments)
            {
                var updatedPost = entitiesAzure.Posts.Add(new Post
                {
                    Title = pair.Post.Title,
                    Author = pair.Post.Author,
                    Content = pair.Post.Content,
                    Created = pair.Post.Created,
                    Type = pair.Post.Type
                });
                entitiesAzure.SaveChanges();
                if (pair.Comments == null)
                    continue;
                foreach (var comment in pair.Comments)
                    entitiesAzure.Comments.Add(new Comment
                    {
                        Author = comment.Author,
                        Created = comment.Created,
                        Text = comment.Text,
                        PostID = updatedPost.ID
                    });
                entitiesAzure.SaveChanges();
            }
        }
    }
}
