using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace DbConverter.GG12to13.Models
{
	class Post : TableEntity
	{
		[IgnoreProperty, JsonIgnore]
		public int Id { get; set; }

		public string Title { get; set; }
		[IgnoreProperty]
		public string Key { get { return RowKey; } set { RowKey = value; } }

		public string Author { get { return (string)Author_ID; } set { Author_ID = value == "Peter" ? 2 : 1; } }
		public DateTime Date { get; set; }
		public string Content { get; set; }
		public int WordCount { get; set; }
		public bool IsStory { get; set; }

		private string author = "Christopher";
		[IgnoreProperty, JsonIgnore]
		public object Author_ID { get { return author; } set { if ((int)value == 2) author = "Peter"; } }

		[IgnoreProperty, JsonProperty(propertyName: "Comments")]
		public Comment[] CommentsRaw { get; set; }

		public int CommentCount { get { return CommentsRaw.Length; } set { } }
		[JsonIgnore]
		public string Comments { get { return JsonConvert.SerializeObject(CommentsRaw); } set { } }

		public Post()
		{
			PartitionKey = "";
		}

		public IndexPost AsIndexPost()
		{
			return new IndexPost
			{
				Title = Title,
				Key = Key,
				Author = Author,
				Date = Date,
				Content = Content,
				WordCount = WordCount,
				IsStory = IsStory,
				CommentCount = CommentCount,
				Comments = Comments
			};
		}
	}

	[SerializePropertyNamesAsCamelCase]
	class IndexPost
	{
		public string Title { get; set; }
		public string Key { get; set; }

		public string Author { get; set; }
		public DateTimeOffset Date { get; set; }
		public string Content { get; set; }
		public int WordCount { get; set; }
		public bool IsStory { get; set; }

		public int CommentCount { get; set; }
		public string Comments { get; set; }
	}
}
