using GrislyGrotto.App.Data;
using System.Data.Entity;

namespace GrislyGrotto.DbConverterCeToServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ceContext = new GrottoContext("sqlCe", false);
            var azureContext = new GrottoContext("sqlAzure");

            var allPosts = ceContext.Posts.Include(o => o.Author).Include(o => o.Comments)
                .ToArrayAsync().Result;

            azureContext.Posts.AddRange(allPosts);
            azureContext.SaveChangesAsync().Wait();
        }
    }
}
