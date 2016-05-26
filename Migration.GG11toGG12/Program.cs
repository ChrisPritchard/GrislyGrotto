using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Migration.GG11toGG12.GG11Data;
using Migration.GG11toGG12.StorageBlobEntities;

namespace Migration.GG11toGG12
{
    class Program
    {
        static void Main()
        {
            var context = new GrislyGrottoContext();
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=grislygrotto;AccountKey=***REMOVED***=");
            var blobClient = storageAccount.CreateCloudBlobClient();

            //var multiples = context.Posts.Select(p => new{p.Title, p.ID, p.Author.DisplayName})
            //    .ToArray().GroupBy(p => Regex.Replace(p.Title.Replace(" ", "-"), "[^A-Za-z0-9 -]+", "").ToLower()).Where(p => p.Count() > 1)
            //    .ToArray()
            //    .Select(p => p.ToArray())
            //    .ToArray();
            //File.WriteAllLines("duplicates.csv", multiples.Select(o => string.Concat(o.Select(f => f.ID + "," + f.Title + "," + f.DisplayName + ","))));

            UploadPosts(context, blobClient);
            //UploadStories(context, blobClient);
            //UploadArchives(context, blobClient);
            //UploadTags(context, blobClient);
        }

        private static string Serialize<T>(T obj, DataContractJsonSerializer dataContractSerializer = null)
        {
            if(dataContractSerializer == null)
                dataContractSerializer = new DataContractJsonSerializer(typeof(T));

            var memoryStream = new MemoryStream();
            dataContractSerializer.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;
            return new StreamReader(memoryStream).ReadToEnd();
        }

        private static void UploadTags(GrislyGrottoContext context, CloudBlobClient blobClient)
        {
            var posts = context.Posts
                .Include("Author")
                .Include("Tags")
                .Select(o => new
                {
                    o.Title,
                    o.Author.Username,
                    o.Created,
                    Tags = o.Tags.Select(t => t.Text).ToList()
                }).ToArray();
            var tags = context.Tags.ToArray()
                .Select(o => new StorageBlobEntities.Tag
            {
                Name = o.Text,
                Posts =
                    posts.Where(p => p.Tags.Any(t => t.Equals(o.Text)))
                    .OrderByDescending(p => p.Created)
                    .Select(p => new PostInfo(p.Title, p.Username, p.Created, p.Tags.ToArray()))
                    .ToList()
            }).OrderByDescending(t => t.Count).ToArray();
            var data = Serialize(tags);

            var metadataContainer = blobClient.GetContainerReference("metadata");
            var blockBlob = metadataContainer.GetBlockBlobReference("tags");
            blockBlob.UploadText(data);
        }

        private static void UploadArchives(GrislyGrottoContext context, CloudBlobClient blobClient)
        {
            var posts = context.Posts
                .Include("Author")
                .Select(o => new
                {
                    o.Title,
                    o.Author.Username,
                    o.Created,
                    Tags = o.Tags.Select(t => t.Text).ToList()
                }).ToArray();

            var months = new CultureInfo("en-NZ").DateTimeFormat.MonthNames;
            var years = posts.GroupBy(o => o.Created.Year)
                .OrderByDescending(o => o.Key)
                .Select(o => new Year
                {
                    Name = o.Key.ToString(),
                    Months = o.GroupBy(f => f.Created.Month).OrderByDescending(m => m.Key).Select(m => new Year.Month
                    {
                        Name = months[m.Key - 1],
                        Count = m.Count(),
                        Posts = m.OrderByDescending(p => p.Created)
                            .Select(n => new PostInfo(n.Title, n.Username, n.Created, n.Tags.ToArray())).ToList()
                    }).ToList()
                }).ToArray();

            var data = Serialize(years);

            var metadataContainer = blobClient.GetContainerReference("metadata");
            var blockBlob = metadataContainer.GetBlockBlobReference("archives");
            blockBlob.UploadText(data);
        }

        private static void UploadStories(GrislyGrottoContext context, CloudBlobClient blobClient)
        {
            var stories = context.Posts
                .Include("Author")
                .Where(o => o.Type == PostType.Story)
                .OrderByDescending(p => p.Created)
                .Select(o => new
                {
                    o.Title,
                    o.Author.Username,
                    o.Created,
                    Tags = o.Tags.Select(t => t.Text).ToList(),
                    o.WordCount
                }).ToArray()
                .Select(o => new PostInfo(o.Title, o.Username, o.Created, o.Tags.ToArray(), o.WordCount)).ToArray();
            var data = Serialize(stories);

            var metadataContainer = blobClient.GetContainerReference("metadata");
            var blockBlob = metadataContainer.GetBlockBlobReference("stories");
            blockBlob.UploadText(data);
        }

        private static void UploadPosts(GrislyGrottoContext context, CloudBlobClient blobClient)
        {
            var posts = context.Posts
                .Include("Author")
                .Include("Comments")
                .Include("Tags").OrderByDescending(p => p.Created).ToArray()
                .Select(p => new StorageBlobEntities.Post(p)).ToArray();

            //var postsContainer = blobClient.GetContainerReference("posts");
            //var dataContractSerializer = new DataContractJsonSerializer(typeof(StorageBlobEntities.Post));
            //foreach (var post in posts)
            //{
            //    var data = Serialize(post, dataContractSerializer);
            //    var blockBlob = postsContainer.GetBlockBlobReference(post.Id);
            //    blockBlob.UploadText(data);
            //}

            var latestIds = posts.Take(5).Select(o => o.Id).ToArray();
            var metadataContainer = blobClient.GetContainerReference("metadata");
            var latestBlob = metadataContainer.GetBlockBlobReference("latestpostids");
            latestBlob.UploadText(Serialize(latestIds));
        }
    }
}