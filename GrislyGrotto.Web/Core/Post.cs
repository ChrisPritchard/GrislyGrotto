using System;
using System.Collections.Generic;

namespace GrislyGrotto.Web.Core
{
    public enum PostStatus
    {
        Draft, Published
    }

    public class Post
    {
        public int ID { get; set; }

        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }

        public IEnumerable<Comment> Comments { get; set; }

        public PostStatus Status { get; set; }
        public User User { get; set; }
    }
}