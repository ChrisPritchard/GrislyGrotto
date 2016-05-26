using System.Data.Entity;

namespace GrislyGrotto.Backup.GG10.Data
{
    class GrislyGrottoContext : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Asset> Assets { get; set; }

        public GrislyGrottoContext(string connectionString)
            : base(connectionString)
        { }
    }
}
