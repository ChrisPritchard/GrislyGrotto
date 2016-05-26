using System;

namespace GrislyGrotto.Data
{
    public class Comment
    {
        public int ID { get; set; }

        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }

        public virtual Post Post { get; set; }
    }
}