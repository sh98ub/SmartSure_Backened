using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrasturcture.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.FullName).HasMaxLength(200);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
