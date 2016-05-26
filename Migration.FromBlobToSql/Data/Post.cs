using System;
using System.Collections.Generic;

namespace GrislyGrotto.Models
{
    public class Post
    {
        public int? ID { get; set; }
        public string Key { get; set; }

        public string Title { get; set; }
        public virtual User Author { get; set; }
        public DateTime Date { get; set; }

        public string Content { get; set; }

        public int WordCount { get; set; }
        public string[] Tags { get; set; }
        public bool IsStory { get; set; }

        public virtual List<Comment> Comments { get; set; }
    }
}