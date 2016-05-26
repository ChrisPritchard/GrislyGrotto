using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GrislyGrotto.Backup
{
    class Program
    {
		const string _serviceName = "SearchServiceName";
		const string _serviceApiKey = "SearchServiceAPIKey";
		const string _indexName = "SearchServiceIndexName";

        const string _connectionStringName = "GrislyGrottoAzureStorage";
        const string _containerName = "backups";

        static void Main()
        {
            var serviceName = ConfigurationManager.AppSettings[_serviceName];
            var serviceApiKey = ConfigurationManager.AppSettings[_serviceApiKey];
            var indexName = ConfigurationManager.AppSettings[_indexName];

            var searchServiceClient = new SearchServiceClient(serviceName, new SearchCredentials(serviceApiKey));
            var indexClient = searchServiceClient.Indexes.GetClient(indexName);

            var allPosts = new List<Post>();
            var i = 0;
            while (true)
            {
                var results = indexClient.Documents.Search<Post>("*", new SearchParameters 
                { 
                    Skip = i * 1000,
                    Top = 1000
                });
                var posts = results.Select(o => o.Document).ToArray();
                allPosts.AddRange(posts);
                if (posts.Length < 1000)
                    break;
                i++;
            }

            allPosts = allPosts.OrderBy(o => o.Date).ToList();
            var postsAsJson = JsonConvert.SerializeObject(allPosts);

            var accessKey = ConfigurationManager.ConnectionStrings[_connectionStringName].ToString();
            var account = CloudStorageAccount.Parse(accessKey);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(_containerName);

            var fileName = $"posts_{DateTime.UtcNow.ToString("dd-MM-yy")}.json";
            var blob = container.GetBlockBlobReference(fileName);
            blob.UploadText(postsAsJson);
        }
    }
}
