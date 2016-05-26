using GrislyGrotto.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GrislyGrotto.DeadLinksFixer
{
    class Program
    {
        const string _serviceName = "SearchServiceName";
        const string _serviceApiKey = "SearchServiceAPIKey";
        const string _indexName = "SearchServiceIndexName";

        static void Main(string[] args)
        {
            var serviceName = ConfigurationManager.AppSettings[_serviceName];
            var serviceApiKey = ConfigurationManager.AppSettings[_serviceApiKey];
            var indexName = ConfigurationManager.AppSettings[_indexName];

            var searchServiceClient = new SearchServiceClient(serviceName, new SearchCredentials(serviceApiKey));
            var indexClient = searchServiceClient.Indexes.GetClient(indexName);

            var imagePosts = new List<Post>();
            for (var i = 0; i < 3; i++)
            {
                var results = indexClient.Documents.Search<Post>("*", new SearchParameters
                {
                    Top = 1000,
                    Skip = i * 1000
                }).Results.Select(o => o.Document).Where(o => o.Content.Contains("/usercontent/")).ToArray();
                imagePosts.AddRange(results);
            }

            foreach (var post in imagePosts)
            {
                post.Content = post.Content.Replace("/usercontent/", "http://grislygrotto.blob.core.windows.net/usercontentpub/");
                Task.WaitAll(PostService.Current.CreateOrUpdate(post));
            }
        }
    }
}
