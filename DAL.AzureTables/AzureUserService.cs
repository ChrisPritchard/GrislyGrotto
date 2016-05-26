using System.Linq;
using GrislyGrotto.DAL.AzureTables.Entities;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables
{
    public class AzureUserService : IUserService
    {
        private readonly TableServiceContext context;

        public AzureUserService(CloudTableClient client, TableServiceContext globalContext)
        {
            context = globalContext;
            client.CreateTableWithSchemaIfDoesntExist("Users", context, new UserEntity { FullName = "schema", LoginName = string.Empty, LoginPassword = string.Empty});
        }

        public User GetUserByFullName(string fullName)
        {
            var query = context.CreateQuery<UserEntity>("Users");
            var user = query.SingleOrDefault(existingUser => existingUser.FullName.Equals(fullName));
            return user == null ? null : user.AsDomainEntity();
        }

        public User Validate(string loginName, string loginPassword)
        {
            var query = context.CreateQuery<UserEntity>("Users");
            var user = query.SingleOrDefault(existingUser => existingUser.LoginName.Equals(loginName) && existingUser.LoginPassword.Equals(loginPassword));
            return user == null ? null : user.AsDomainEntity();
        }

        public User GetUserByLoginName(string loginName)
        {
            var query = context.CreateQuery<UserEntity>("Users");
            var user = query.Where(existingUser => existingUser.LoginName.Equals(loginName)).ToList();
            return user.Count == 0 ? null : user.First().AsDomainEntity();
        }
    }
}