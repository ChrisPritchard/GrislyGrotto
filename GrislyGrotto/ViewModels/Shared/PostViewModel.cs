using System;
using System.Collections.Generic;

namespace GrislyGrotto.ViewModels.Shared
{
    public enum PostTypeViewModel
    {
        Normal, Story
    }

    public class PostViewModel
    {
        public int ID { get; set; }
        public virtual UserViewModel Author { get; set; }

        public string Title { get; set; }
        public DateTime Created { get; set; }
        public PostTypeViewModel Type { get; set; }

        public string Content { get; set; }
        public int WordCount { get; set; }

        public virtual List<CommentViewModel> Comments { get; set; }
        public virtual List<SingleTagViewModel> Tags { get; set; }
    }
}