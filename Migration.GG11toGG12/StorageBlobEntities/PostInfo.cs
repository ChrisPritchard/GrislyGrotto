using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Migration.GG11toGG12.StorageBlobEntities
{
    [DataContract]
    public class PostInfo
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "date")]
        public string Date { get; set; }
        [DataMember(Name = "isodate")]
        public string IsoDate { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "wordcount")]
        public int WordCount { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }
        [DataMember(Name = "commentcount")]
        public int CommentCount { get; set; }

        private const string shortFormDateFormat = "d MMMM yyyy";

        public PostInfo(GG11Data.Post source)
        {
            Id = CreateIdNameFrom(source.Title);

            Author = source.Author.Username;
            Title = source.Title;
            IsoDate = source.Created.ToString("s") + "+1300";
            Date = source.Created.ToString(shortFormDateFormat);
            Type = source.Type.ToString();
            WordCount = source.WordCount;
            Tags = source.Tags.Select(o => o.Text).ToArray();
            CommentCount = source.Comments != null ? source.Comments.Count() : 0;
        }

        public PostInfo(string title, string username, DateTime created, string[] tags)
        {
            Id = CreateIdNameFrom(title);
            Author = username;
            Title = title;
            IsoDate = created.ToString("s") + "1300";
            Date = created.ToString(shortFormDateFormat);
            Tags = tags;
        }

        public PostInfo(string title, string username, DateTime created, string[] tags, int wordCount)
            : this(title, username, created, tags)
        {
            WordCount = wordCount;
        }

        private string CreateIdNameFrom(string title)
        {
            return Regex.Replace(title.Replace(" ", "-"), "[^A-Za-z0-9 -]+", "").ToLower();
        }
    }
}