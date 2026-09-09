using Microsoft.EntityFrameworkCore;
using S3.Gateway.Entities;

namespace S3.Gateway.Data
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Áp dụng cho tất cả string properties trong tất cả entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var entityBuilder = modelBuilder.Entity(entityType.ClrType);

                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string)))
                {
                    entityBuilder.Property(property.Name).IsRequired(false);
                }
            }

            modelBuilder.Entity<Partner>(entity =>
            {
                entity.Property(item => item.Code).IsRequired().HasMaxLength(50);
                entity.Property(item => item.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(item => item.Code).IsUnique();
            });

            modelBuilder.Entity<PartnerCredential>(entity =>
            {
                entity.Property(item => item.AccessKey).IsRequired().HasMaxLength(100);
                entity.Property(item => item.SecretKeyHash).IsRequired().HasMaxLength(100);
                entity.Property(item => item.Scopes).HasMaxLength(1000);
                entity.HasIndex(item => item.AccessKey).IsUnique();
                entity.HasOne(item => item.Partner)
                    .WithMany(item => item.Credentials)
                    .HasForeignKey(item => item.PartnerID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CallbackRouting>(entity =>
            {
                entity.Property(item => item.ClientRequestID).HasMaxLength(100);
                entity.HasOne(item => item.Partner)
                    .WithMany()
                    .HasForeignKey(item => item.PartnerID)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.PartnerCredential)
                    .WithMany()
                    .HasForeignKey(item => item.PartnerCredentialID)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(item => new { item.PartnerID, item.Target, item.ClientRequestID })
                    .IsUnique()
                    .HasFilter("[PartnerID] IS NOT NULL AND [ClientRequestID] IS NOT NULL");
            });
        }

        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<CallbackRouting> CallbackRoutings { get; set; }
        public DbSet<CallbackRoutingLog> CallbackRoutingLogs { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerCredential> PartnerCredentials { get; set; }
    }
}
