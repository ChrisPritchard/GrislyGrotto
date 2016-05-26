using System.Runtime.Serialization;

namespace Migration.GG11toGG12.StorageBlobEntities
{
    [DataContract]
    public class Comment
    {
        [DataMember(Name = "author")]
        public string Author { get; set; }
        [DataMember(Name = "date")]
        public string Date { get; set; }
        [DataMember(Name = "isodate")]
        public string IsoDate { get; set; }
        [DataMember(Name = "content")]
        public string Content { get; set; }

        public Comment(GG11Data.Comment source)
        {
            Author = source.Author;
            IsoDate = source.Created.ToString("s");
            Date = source.Created.ToString("hh:mm tt, ddddd, d MMMM, yyyy");
            Content = source.Content;
        }
    }
}