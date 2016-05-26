using System;

namespace GrislyGrotto.Infrastructure.Domain
{
    public class Comment
    {
        public int PostID { get; private set; }
        public DateTime EntryDate { get; private set; }
        public string Author { get; private set; }
        public string Text { get; private set; }

        public Comment(int postID, DateTime entryDate, string author, string text)
        {
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Text = text;
        }
    }
}
