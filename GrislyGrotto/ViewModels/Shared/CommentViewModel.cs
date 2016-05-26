using System;

namespace GrislyGrotto.ViewModels.Shared
{
    public class CommentViewModel
    {
        public int ID { get; set; }

        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }
    }
}