using ClaimService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimService.Infrastructure.Data
{
    public class ClaimDbContext : DbContext
    {
        public ClaimDbContext(DbContextOptions<ClaimDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ClaimAmount).HasColumnType("decimal(18,2)");
                entity.Property(c => c.ApprovedPayoutAmount).HasColumnType("decimal(18,2)");
            });
        }
    }
}
