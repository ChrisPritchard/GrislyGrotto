using System.Data.Entity;

namespace GrislyGrotto.Models
{
    public class GrislyGrottoContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public GrislyGrottoContext()
            : base("name=GrislyGrottoDatabase")
        {
            Configuration.ProxyCreationEnabled = false;
        }
    }
}