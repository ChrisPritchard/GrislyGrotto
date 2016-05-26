using System;
using System.Collections.Generic;

namespace GrislyGrotto.Data
{
    public enum PostType
    {
        Normal, Story
    }

    public class Post
    {
        public int ID { get; set; }
        public virtual User Author { get; set; }

        public string Title { get; set; }
        public DateTime Created { get; set; }
        public PostType Type { get; set; }

        public string Content { get; set; }
        public int WordCount { get; set; }

        public virtual List<Comment> Comments { get; set; }
        public virtual List<Tag> Tags { get; set; }
    }
}