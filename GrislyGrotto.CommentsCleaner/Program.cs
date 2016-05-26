using GrislyGrotto.Backup;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GrislyGrotto.CommentsCleaner
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

            var bloatedPosts = new List<Post>();
            for (var i = 0; i < 3; i++)
            {
                var results = indexClient.Documents.Search<Post>("*", new SearchParameters
                {
                    Select = new[] { "key", "commentCount", "comments" },
                    Top = 1000,
                    Skip = i * 1000
                }).Results.Select(o => o.Document).Where(o => o.CommentCount >= 20).ToArray();
                bloatedPosts.AddRange(results);
            }

            var count = bloatedPosts.Count();
        }
    }
}
