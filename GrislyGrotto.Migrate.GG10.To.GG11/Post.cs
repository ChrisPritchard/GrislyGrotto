using System;

namespace GrislyGrotto.Migrate.GG10.To.GG11
{
    public class Post
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
