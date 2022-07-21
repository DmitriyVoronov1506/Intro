using Microsoft.EntityFrameworkCore;

namespace WebApplicationClassWork.DAL.Context
{
    public class IntroContext : DbContext
    {
        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Entities.Topic> Topics { get; set; }
        public DbSet<Entities.Article> Articles { get; set; }

        public IntroContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
        }

    }
}
