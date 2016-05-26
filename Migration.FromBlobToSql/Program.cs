using GrislyGrotto.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Migration.FromBlobToSql
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var context = new GrislyGrottoContext();

            var users = GetOrCreateUsers(context);
            LoadPostsFromJSON(context, users);
        }

        private static void LoadPostsFromJSON(GrislyGrottoContext context, Dictionary<string, User> users)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var existingPosts = context.Posts.Select(p => p.Key).ToArray();

            var files = Directory.GetFiles(dialog.SelectedPath);
            var newPosts = new List<Post>();
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                if (text[0] != '{' || text[text.Length - 1] != '}')
                    continue;
                dynamic blobPost = JObject.Parse(text);
                if (existingPosts.Contains((string)blobPost.id))
                    continue;

                var dbPost = new Post
                {
                    Key = blobPost.id,
                    Title = blobPost.title,
                    Author = users[(string)blobPost.author],
                    Date = ParseDate((string)blobPost.isodate),
                    Content = blobPost.content,
                    WordCount = blobPost.wordcount,
                    Tags = ((JArray)blobPost.tags).Select(o => (string)o).ToArray(),
                    IsStory = blobPost.type == "Story"
                };

                dbPost.Comments = ((JArray)blobPost.comments)
                    .Select(o => (dynamic)o).Select(o =>
                    new Comment
                    {
                        Author = o.author,
                        Content = o.content,
                        Date = ParseDate((string)o.isodate)
                    }).ToList<Comment>();
                dbPost.Comments = dbPost.Comments.OrderBy(o => o.Date).ToList();

                newPosts.Add(dbPost);
            }

            newPosts = newPosts.OrderBy(o => o.Date).ToList();
            context.Posts.AddRange(newPosts);
            context.SaveChanges();
        }

        static List<string> dates = new List<string>();

        private static DateTime ParseDate(string rawDate)
        {
            var segments = rawDate.Split(' ');
            var numbers = segments.SelectMany(o => o.Split('/', ':').Select(int.Parse)).ToArray();
            return new DateTime(numbers[2], numbers[0], numbers[1], numbers[3], numbers[4], numbers[5]);
        }

        private static Dictionary<string, User> GetOrCreateUsers(GrislyGrottoContext context)
        {
            if (context.Users.Any())
                return context.Users.ToDictionary<User, string>(o => o.Username);

            var salts = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            context.Users.Add(new User
            {
                Username = "aquinas",
                Password = Salting.GetSaltedPassword("***REMOVED***", salts[0]),
                PasswordSalt = salts[0],
                DisplayName = "Christopher",
                ImageUrl = "http://graph.facebook.com/christopher.j.pritchard/picture"
            });
            context.Users.Add(new User
            {
                Username = "pdc",
                Password = Salting.GetSaltedPassword("***REMOVED***", salts[1]),
                PasswordSalt = salts[1],
                DisplayName = "Peter",
                ImageUrl = "http://graph.facebook.com/peter.coleman.5/picture"
            });
            context.SaveChanges();

            return context.Users.ToDictionary<User, string>(o => o.Username);
        }
    }
}

