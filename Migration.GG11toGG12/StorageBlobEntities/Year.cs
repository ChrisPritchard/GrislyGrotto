
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Migration.GG11toGG12.StorageBlobEntities
{
    [DataContract]
    public class Year
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "months")]
        public List<Month> Months { get; set; }

        [DataContract]
        public class Month
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
            [DataMember(Name = "count")]
            public int Count { get; set; }

            [DataMember(Name = "posts")]
            public List<PostInfo> Posts { get; set; }
        }
    }
}
