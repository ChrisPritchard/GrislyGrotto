using System.Data.Entity;

namespace GrislyGrotto.App.Data
{
    public class GrottoContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet <Comment> Comments { get; set; }
        public DbSet <Author> Authors { get; set; }

        public GrottoContext()
            : base("GrottoContext")
        { }

        public GrottoContext(string nameOrConnectionString, bool proxyCreationEnabled = true)
            : base(nameOrConnectionString)
        {
            Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        }
    }
}