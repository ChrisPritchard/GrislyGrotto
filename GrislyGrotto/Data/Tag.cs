using System.Collections.Generic;

namespace GrislyGrotto.Data
{
    public class Tag
    {
        public int ID { get; set; }
        public string Text { get; set; }

        public virtual List<Post> Posts { get; set; }
    }
}