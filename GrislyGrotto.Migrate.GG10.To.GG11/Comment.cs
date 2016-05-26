using System;

namespace GrislyGrotto.Migrate.GG10.To.GG11
{
    public class Comment
    {
        public int ID { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }
        public int PostID { get; set; }
    }
}
