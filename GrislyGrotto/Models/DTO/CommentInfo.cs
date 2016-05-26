using System;

namespace GrislyGrotto.Models.DTO
{
    public class CommentInfo
    {
        public int CommentID { get; private set; }
        public int PostID { get; private set; }
        public DateTime EntryDate { get; private set; }
        public string Author { get; private set; }
        public string Text { get; private set; }

        public CommentInfo(int postID, DateTime entryDate, string author, string text)
        {
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Text = text;
        }

        public CommentInfo(int commentID, int postID, DateTime entryDate, string author, string text)
        {
            CommentID = commentID;
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Text = text;
        }
    }
}
