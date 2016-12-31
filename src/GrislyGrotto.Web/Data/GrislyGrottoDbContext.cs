using Microsoft.EntityFrameworkCore;

namespace GrislyGrotto
{
    public class GrislyGrottoDbContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public GrislyGrottoDbContext(DbContextOptions<GrislyGrottoDbContext> options)
            : base(options)
        { }
    }
}