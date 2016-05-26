using System.Data.Entity;

namespace GrislyGrotto.Data
{
    public class GrislyGrottoContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public GrislyGrottoContext()
            : base("name=GrislyGrottoDatabase")
        { }
    }
}