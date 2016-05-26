using GrislyGrotto.Models;
using Hyak.Common;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace GrislyGrotto
{
	public class SearchClient
	{
		private readonly SearchIndexClient indexClient;

		const string _serviceName = "SearchServiceName";
		const string _serviceApiKey = "SearchServiceAPIKey";
		const string _indexName = "SearchServiceIndexName";

		private SearchClient()
		{
			var serviceName = WebConfigurationManager.AppSettings[_serviceName];
			var serviceApiKey = WebConfigurationManager.AppSettings[_serviceApiKey];
			var indexName = WebConfigurationManager.AppSettings[_indexName];

			var searchServiceClient = new SearchServiceClient(serviceName, new SearchCredentials(serviceApiKey));
			indexClient = searchServiceClient.Indexes.GetClient(indexName);
		}

        public async Task<bool> Exists(string key)
        {
            try
            {
                var existing = await indexClient.Documents.GetAsync<Post>(key, new[] { "title" });
                return existing != null && existing.Document != null;
            }
            catch(CloudException)
            {
                return false;
            }
        }

		public async Task<Post> Get(string key)
		{
			return (await indexClient.Documents.GetAsync<Post>(key)).Document;
		}

        public async Task CreateOrUpdate(Post updatedPost)
        {
            var update = IndexAction.Create(IndexActionType.MergeOrUpload, updatedPost);
            var batch = new IndexBatch<Post>(new[] { update });
            await indexClient.Documents.IndexAsync(batch);
        }

        public async Task<SearchResult<Post>[]> Search(string searchText = "*", string filter = null, string orderBy = null, int count = 1, int skip = 0, 
            string[] returnFields = null, string[] searchFields = null)
		{
			var parameters = new SearchParameters
			{
				Filter = !string.IsNullOrWhiteSpace(filter) ? filter : null,
                OrderBy = orderBy != null ? new[] { orderBy } : new string[0],
				Top = count,
				Skip = skip,
                Select = returnFields ?? new string[0], 
                SearchFields = searchFields ?? new string[0],
                HighlightFields = searchFields ?? new string[0]
			};

            var search = await indexClient.Documents.SearchAsync<Post>(searchText, parameters);
            return search.Results.ToArray();
		}

		private static SearchClient searchClient;
		public static SearchClient Current
		{
			get { return searchClient ?? (searchClient = new SearchClient()); }
		}
	}
}