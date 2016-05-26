using System.Xml.Linq;
using GrislyGrotto.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DatabaseComment = GrislyGrotto.Data.Comment;
using DatabasePost = GrislyGrotto.Data.Post;

namespace GrislyGrotto.Migrate.GG10.To.GG11
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 && Debugger.IsAttached)
                args = new[] { "Backup_635068978099975099.xml" };

            if (args.Length == 0 || !File.Exists(args[0]))
                return;

            var xml = File.ReadAllText(args[0]);
            var serializer = new XmlSerializer(typeof(Container));
            var container = (Container)serializer.Deserialize(new StringReader(xml));

            var posts = container.Posts
                .Select(p => new
                {
                    Post = p,
                    Comments = container.Comments.Where(c => c.PostID == p.ID).ToArray()
                }).ToArray();

            var context = new GrislyGrottoContext();
            var startTime = DateTime.Now;
            Console.Write("Starting...");

            var quotes = XDocument.Load("Quotes.xml");
            foreach (var quote in quotes.Root.Elements())
                context.Quotes.Add(new Quote { Author = quote.Attribute("Author").Value.Trim(), Text = quote.Value.Trim() });

            var users = GetUsers(context);

            foreach (var postSet in posts)
            {
                var dbPost = new DatabasePost
                {
                    Title = postSet.Post.Title,
                    Created = postSet.Post.Created,
                    Content = postSet.Post.Content,
                    Type = postSet.Post.Type.Equals("Story") ? PostType.Story : PostType.Normal
                };
                if (!users.ContainsKey(postSet.Post.Author))
                {
                    dbPost.Author = users["Christopher"];
                    dbPost.Content = string.Format("<p><i>Originally posted by {0}</i></p>", postSet.Post.Author)
                        + dbPost.Content;
                }
                else
                    dbPost.Author = users[postSet.Post.Author];
                dbPost.WordCount = GetWordCount(dbPost.Content);
                context.Posts.Add(dbPost);

                foreach (var comment in postSet.Comments)
                {
                    var dbComment = new DatabaseComment
                    {
                        Author = comment.Author,
                        Created = comment.Created,
                        Content = comment.Text,
                        Post = dbPost
                    };
                    context.Comments.Add(dbComment);
                }
            }

            context.SaveChanges();

            Console.WriteLine("Finished. {0} seconds", DateTime.Now.Subtract(startTime).TotalSeconds);
            if (Debugger.IsAttached)
                Console.ReadKey(true);
        }

        private static int GetWordCount(string content)
        {
            var stripped = Regex.Replace(content, @"<[^>]*>", string.Empty);
            return stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        private static Dictionary<string, User> GetUsers(GrislyGrottoContext context)
        {
            if (!context.Users.Any())
            {
                var newUsers = new[] 
                {
                    new User { DisplayName = "Christopher", Username = "aquinas", Password = "***REMOVED***" },
                    new User { DisplayName = "Peter", Username = "pdc", Password = "***REMOVED***" }
                };

                foreach (var newUser in newUsers)
                {
                    var salt = newUser.Salt = User.GetSalt();
                    newUser.Password = User.GetSaltedPassword(newUser.Password, salt);
                    context.Users.Add(newUser);
                }

                context.SaveChanges();
            }

            return context.Users.ToDictionary(k => k.DisplayName, v => v);
        }
    }

    public class Container
    {
        public Post[] Posts { get; set; }
        public Comment[] Comments { get; set; }
    }
}
