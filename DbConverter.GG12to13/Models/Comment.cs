using Newtonsoft.Json;
using System;

namespace DbConverter.GG12to13.Models
{
	class Comment
	{
		[JsonIgnore]
		public int Post_ID { get; set; }

		public string Author { get; set; }
		public DateTime Date { get; set; }
		public string Content { get; set; }
	}
}
