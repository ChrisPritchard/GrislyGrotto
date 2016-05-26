using System;

namespace Migration.GG11toGG12.GG11Data
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