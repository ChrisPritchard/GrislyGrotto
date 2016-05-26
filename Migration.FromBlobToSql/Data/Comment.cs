using System;

namespace GrislyGrotto.Models
{
    public class Comment
    {
        public int? ID { get; set; }

        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
    }
}
