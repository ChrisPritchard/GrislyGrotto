using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace GrislyGrotto.Models
{
    [SerializePropertyNamesAsCamelCase]
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

        public Dictionary<string, string> Highlights { get; set; }
	}
}