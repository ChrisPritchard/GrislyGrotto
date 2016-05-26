using System;

namespace GrislyGrotto.Backup
{
    public class Post
    {
        public string Title { get; set; }
        public string Key { get; set; }

        public string Author { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Content { get; set; }
        public int? WordCount { get; set; }
        public bool? IsStory { get; set; }

        public int? CommentCount { get; set; }
        public string Comments { get; set; }
	}
}