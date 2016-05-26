using System;

namespace GrislyGrotto.Framework.Data.Primitives
{
    public class RecentPost
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string TimeSincePosted { get; set; }
        public string Username { get; set; }

        public RecentPost()
        { }

        public RecentPost(Post post)
        {
            ID = post.ID.Value;
            Title = post.Title;
            TimeSincePosted = DateTime.Now.Subtract(post.TimePosted).ToWebFormat();
            Username = post.Username;
        }
    }
}
