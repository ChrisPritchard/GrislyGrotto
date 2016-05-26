using System;

namespace GrislyGrotto.Infrastructure.Domain
{
    public class Post
    {
        public int PostID { get; private set; }

        public DateTime EntryDate { get; private set; }
        public string Author { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }

        public Post(int postID, DateTime entryDate, string author, string title, string content)
        {
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Title = title;
            Content = content;
        }
    }
}
