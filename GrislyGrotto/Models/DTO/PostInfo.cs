using System;

namespace GrislyGrotto.Models.DTO
{
    public class PostInfo
    {
        public int PostID { get; private set; }

        public DateTime EntryDate { get; private set; }
        public UserInfo Author { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        
        public int CommentCount { get; private set; }
        public CommentInfo[] Comments { get; private set; }

        public PostInfo(int postID, string title, string content)
        {
            PostID = postID;
            Title = title;
            Content = content;
        }

        public PostInfo(int postID, DateTime entryDate, string title)
        {
            PostID = postID;
            EntryDate = entryDate;
            Title = title;
        }

        public PostInfo(DateTime entryDate, UserInfo author, string title, string content)
        {
            EntryDate = entryDate;
            Author = author;
            Title = title;
            Content = content;
        }

        public PostInfo(int postID, DateTime entryDate, UserInfo author, string title, string content, int commentCount)
        {
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Title = title;
            Content = content;
            CommentCount = commentCount;
        }

        public PostInfo(int postID, DateTime entryDate, UserInfo author, string title, string content, CommentInfo[] comments)
        {
            PostID = postID;
            EntryDate = entryDate;
            Author = author;
            Title = title;
            Content = content;
            CommentCount = comments.Length;
            Comments = comments;
        }
    }
}
