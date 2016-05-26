using System;

namespace GrislyGrotto.Core.Entities
{
    public class Comment
    {
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
    }
}