using GrislyGrotto.App.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Console;

namespace GrislyGrotto.DbConverterSearchToSql
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                WriteLine("first argument must be a valid json file to read");
                return;
            }

            var backupText = File.ReadAllText(args[0]);
            dynamic backupData = JsonConvert.DeserializeObject(backupText);
            var db = new GrottoContext();

            if (db.Database.Exists())
                db.Database.Delete();
            db.Database.Create();

            var christopher = new Author
            {
                Username = "aquinas",
                Password = "***REMOVED***",
                DisplayName = "Christopher",
                ImageUrl = "http://grislygrotto.blob.core.windows.net/usercontentpub/facebook-small-chris.jpg"
            };
            var peter = new Author
            {
                Username = "pdc",
                Password = "***REMOVED***",
                DisplayName = "Peter",
                ImageUrl = "http://grislygrotto.blob.core.windows.net/usercontentpub/facebook-small-pete.jpg"
            };
            db.Authors.Add(christopher);
            db.Authors.Add(peter);
            db.SaveChangesAsync();
            
            for (var i = 0; i < backupData.Count; i++)
            {
                var backupPost = backupData[i];
                var dbPost = new Post
                {
                    Title = backupPost.Title,
                    Content = backupPost.Content,
                    Date = ((DateTime)backupPost.Date).ToUniversalTime(),
                    Author = backupPost.Author == "Christopher" ? christopher : peter,
                    IsStory = backupPost.IsStory == true
                };
                dbPost.UpdateWordCount();
                dbPost.Key = dbPost.TitleAsKey();

                db.Posts.Add(dbPost);

                if (backupPost.Comments != null && backupPost.Comments != "[]")
                {
                    dynamic comments = JsonConvert.DeserializeObject(backupPost.Comments.Value);
                    foreach (var backupComment in comments)
                    {
                        if (string.IsNullOrEmpty((string)backupComment.Content))
                            continue;
                        db.Comments.Add(new Comment
                        {
                            Author = backupComment.Author,
                            Content = backupComment.Content,
                            Date = ((DateTime)backupComment.Date).ToUniversalTime(),
                            Post = dbPost
                        });
                    }
                }

                db.SaveChanges();
                WriteLine($"{i + 1} of {backupData.Count}: {dbPost.Title}");
            }

            WriteLine();
            WriteLine("operation completed");
            ReadKey(true);
        }
    }
}
