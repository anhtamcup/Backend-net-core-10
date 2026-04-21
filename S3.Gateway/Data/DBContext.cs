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
        }

        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<CallbackRouting> CallbackRoutings { get; set; }
        public DbSet<CallbackRoutingLog> CallbackRoutingLogs { get; set; }
    }
}
