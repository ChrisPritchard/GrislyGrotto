using System;

namespace GrislyGrotto.Data.Primitives
{
    public class Post
    {
        public int? Id { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public DateTime TimePosted { get; set; }
        public string TimePostedWeb { get { return TimePosted.ToWebFormat(); } }
        public string Content { get; set; }

        public Comment[] Comments { get; set; }
        public bool IsStory { get; set; }

        public Post()
        {
            TimePosted = DateTime.Now;
            Comments = new Comment[0];
            IsStory = false;
        }
    }
}