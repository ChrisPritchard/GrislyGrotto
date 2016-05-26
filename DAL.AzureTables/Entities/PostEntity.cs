using System;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables.Entities
{
    public class PostEntity : TableServiceEntity
    {
        public int ID { get; set; }

        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }

        public string Status { get; set; }

        public string UserLoginName { get; set; }

        public PostEntity() : base(string.Empty, Guid.NewGuid().ToString())
        { }
    }
}