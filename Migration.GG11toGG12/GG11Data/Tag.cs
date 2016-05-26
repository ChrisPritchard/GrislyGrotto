using System.Collections.Generic;

namespace Migration.GG11toGG12.GG11Data
{
    public class Tag
    {
        public int ID { get; set; }
        public string Text { get; set; }

        public virtual List<Post> Posts { get; set; }
    }
}