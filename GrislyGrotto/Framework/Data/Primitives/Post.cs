using System;

namespace GrislyGrotto.Framework.Data.Primitives
{
    public class Post
    {
        public int? ID { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public DateTime TimePosted { get; set; }
        public string FormattedTimePosted 
        { 
            get
            {
                return TimePosted.ToWebFormat();  
            }
            set { }
        }
        public string Content { get; set; }
        public string RawContent
        {
            get
            {
                return Content.StripHtml();
            }
            set { }
        }

        public Comment[] Comments { get; set; }
        public bool IsStory { get; set; }
    }
}