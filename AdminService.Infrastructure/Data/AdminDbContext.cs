using AdminService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Actor).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(200);
            });
        }
    }
}
