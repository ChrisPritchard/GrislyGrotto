using System;

namespace GrislyGrotto.Data.Primitives
{
    public class Comment
    {
        public string Author { get; set; }
        public DateTime TimeMade { get; set; }
        public string TimeMadeWeb { get { return TimeMade.ToWebFormat(); } set { } }
        public string Content { get; set; }

        public Comment()
        { }

        public Comment(string author, string content)
        {
            Author = author;
            Content = content;
            TimeMade = DateTime.Now;
        }
    }
}