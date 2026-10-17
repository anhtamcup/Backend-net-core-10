using DD.Gateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Data;

public sealed class GatewayDbContext(DbContextOptions<GatewayDbContext> options) : DbContext(options)
{
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerCredential> PartnerCredentials => Set<PartnerCredential>();
    public DbSet<EkycSession> EkycSessions => Set<EkycSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.RequestId).HasMaxLength(100);
            entity.Property(log => log.Method).HasMaxLength(20);
            entity.Property(log => log.Path).HasMaxLength(2048);
            entity.Property(log => log.IpAddress).HasMaxLength(100);
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => log.RequestId);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(partner => partner.Id);
            entity.Property(partner => partner.Code).HasMaxLength(50).IsRequired();
            entity.Property(partner => partner.Name).HasMaxLength(200).IsRequired();
            entity.Property(partner => partner.CallbackUrl).HasMaxLength(2048);
            entity.HasIndex(partner => partner.Code).IsUnique();
        });

        modelBuilder.Entity<PartnerCredential>(entity =>
        {
            entity.HasKey(credential => credential.Id);
            entity.Property(credential => credential.AccessKey).HasMaxLength(100).IsRequired();
            entity.Property(credential => credential.SecretKey).HasMaxLength(100);
            entity.Property(credential => credential.SecretKeyHash).HasMaxLength(100).IsRequired();
            entity.Property(credential => credential.Scopes).HasMaxLength(1000).IsRequired();
            entity.HasIndex(credential => credential.AccessKey).IsUnique();
            entity.HasOne(credential => credential.Partner)
                .WithMany(partner => partner.Credentials)
                .HasForeignKey(credential => credential.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EkycSession>(entity =>
        {
            entity.HasKey(session => session.SessionId);
            entity.Property(session => session.SessionId).HasMaxLength(36);
            entity.Property(session => session.Provider).HasMaxLength(50).IsRequired();
            entity.Property(session => session.ProviderPartnerId).HasMaxLength(100).IsRequired();
            entity.Property(session => session.CallbackUrl).HasMaxLength(2048).IsRequired();
            entity.Property(session => session.ResultHash).HasMaxLength(64);
            entity.Property(session => session.Status).HasMaxLength(20);
            entity.Property(session => session.Code).HasMaxLength(2);
            entity.HasIndex(session => new { session.PartnerId, session.CreatedAt });
            entity.HasOne(session => session.Partner)
                .WithMany()
                .HasForeignKey(session => session.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
