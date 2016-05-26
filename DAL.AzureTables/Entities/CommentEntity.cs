using System;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables.Entities
{
    public class CommentEntity : TableServiceEntity
    {
        public int PostID { get; set; }

        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }

        public CommentEntity() : base(string.Empty, Guid.NewGuid().ToString())
        { }
    }
}