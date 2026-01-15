namespace DnsResolver.Infrastructure.Data;

using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Aggregates.DdnsTask;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DdnsTask> DdnsTasks => Set<DdnsTask>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // DdnsTask 配置
        modelBuilder.Entity<DdnsTask>(entity =>
        {
            entity.ToTable("DdnsTasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProviderId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProviderSecret).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RecordId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SubDomain).HasMaxLength(100);
            entity.Property(e => e.LastKnownIp).HasMaxLength(50);
            entity.Property(e => e.LastError).HasMaxLength(500);

            // 将 Dictionary 序列化为 JSON 存储
            entity.Property(e => e.ExtraParams)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null));
        });
    }
}
