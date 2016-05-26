using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Migration.GG11toGG12.StorageBlobEntities
{
    [DataContract]
    public class Tag
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "count")]
        public int Count
        {
            get { return Posts.Count; }
            set { }
        }

        [DataMember(Name = "posts")]
        public List<PostInfo> Posts { get; set; }
    }
}
