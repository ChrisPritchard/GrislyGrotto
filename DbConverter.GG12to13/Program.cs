using Dapper;
using DbConverter.GG12to13.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace DbConverter.GG12to13
{
	class Program
	{
		const string _gg12DbConnectionString = "Server=tcp:mhvrgafbmv.database.windows.net,1433;Database=GrislyGrottoDB_v12.8;User ID=grislygrotto_dbuser@mhvrgafbmv;Password=***REMOVED***;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
		const string _azureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=grislygrotto;AccountKey=***REMOVED***=";
		const string _jsonFilename = "posts.json";

		const string _searchName = "hundredmile";
		const string _searchKey = "4BBCD1C6F017B576EB2EAF19DB447FB6";
		const string _indexKey = "grislygrotto";

		static void Main(params string[] args)
		{
			Post[] posts;
			if (File.Exists(_jsonFilename))
				posts = JsonConvert.DeserializeObject<Post[]>(File.ReadAllText(_jsonFilename));
			else
			{
				var connection = new SqlConnection(_gg12DbConnectionString);
				connection.Open();

				posts = connection.Query<Post>("SELECT * FROM Posts").ToArray();
				var comments = connection.Query<Comment>("SELECT * FROM Comments").ToArray();

				foreach (var post in posts)
					post.CommentsRaw = comments.Where(o => o.Post_ID == post.Id).ToArray();
			}

			if (args.Contains("-tablestorage"))
			{
				var storageClient = CloudStorageAccount.Parse(_azureStorageConnectionString);
				var tableClient = storageClient.CreateCloudTableClient();

				var table = tableClient.GetTableReference("poststest");
				table.CreateIfNotExists();

				foreach (var post in posts)
					table.Execute(TableOperation.Insert(post));
			}
			else
				SendToSearch(posts);

			if (!File.Exists(_jsonFilename))
			{
				var json = JsonConvert.SerializeObject(posts);
				File.WriteAllText("posts.json", json);
			}
		}

		private static void SendToSearch(Post[] posts)
		{
			var searchClient = new SearchServiceClient(_searchName, new SearchCredentials(_searchKey));

			if (searchClient.Indexes.Exists(_indexKey))
				searchClient.Indexes.Delete(_indexKey);

			searchClient.Indexes.Create(new Index
			{
				Name = _indexKey,
				Fields = new[] { 
					new Field("key", DataType.String) { IsKey = true },
					new Field("title", DataType.String) { IsSearchable = true },
					new Field("author", DataType.String) { IsSearchable = true, IsFilterable = true },
					new Field("date", DataType.DateTimeOffset) { IsSortable = true },
					new Field("content", DataType.String) { IsSearchable = true },
					new Field("wordCount", DataType.Int32),
					new Field("isStory", DataType.Boolean) { IsFilterable = true },
					new Field("commentCount", DataType.Int32),
					new Field("comments", DataType.String),
				}
			});

			var indexClient = searchClient.Indexes.GetClient(_indexKey);
			var allPosts = posts.Select(o => IndexAction.Create(o.AsIndexPost())).ToArray();
			var batches = new List<IndexBatch<IndexPost>>();
			for (var i = 0; i < allPosts.Length; i += 1000)
			{
				var postsSet = allPosts.Skip(i).Take(1000).ToArray();
				batches.Add(IndexBatch.Create(postsSet));
			}

			try
			{
				foreach(var batch in batches)
					indexClient.Documents.Index(batch);
			}
			catch (IndexBatchException ex)
			{
				Console.WriteLine("Failed to index some of the documents: {0}", string.Join(", ", ex.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)));
			}
		}
	}
}
