using System;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables.Entities
{
    public class UserEntity : TableServiceEntity
    {
        public string FullName { get; set; }

        public string LoginName { get; set; }
        public string LoginPassword { get; set; }

        public UserEntity() : base(string.Empty, Guid.NewGuid().ToString())
        { }
    }
}