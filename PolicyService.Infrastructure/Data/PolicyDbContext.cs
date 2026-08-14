using Microsoft.EntityFrameworkCore;
using PolicyService.Domain;

namespace PolicyService.Infrastructure.Data
{
    public class PolicyDbContext : DbContext
    {
        public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options) { }

        public DbSet<PolicyPlan> PolicyPlans { get; set; } = null!;
        public DbSet<UserPolicy> UserPolicies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PolicyPlan>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
                entity.Property(p => p.BasePremium).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CoverageLimit).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<UserPolicy>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.Property(up => up.PremiumAmount).HasColumnType("decimal(18,2)");
                entity.Property(up => up.CoverageLimit).HasColumnType("decimal(18,2)");
            });
        }
    }
}
