using System.Linq;
using System.Runtime.Serialization;

namespace Migration.GG11toGG12.StorageBlobEntities
{
    [DataContract]
    public class Post : PostInfo
    {
        [DataMember(Name = "content")]
        public string Content { get; set; }
        [DataMember(Name = "comments")]
        public Comment[] Comments { get; set; }

        public Post(GG11Data.Post source)
            : base(source)
        {
            Date = source.Created.ToString("hh:mm tt ddddd d MMMM, yyyy");
            Content = source.Content;
            Comments = source.Comments
                .OrderBy(o => o.Created)
                .Select(o => new Comment(o)).ToArray();
        }
    }
}