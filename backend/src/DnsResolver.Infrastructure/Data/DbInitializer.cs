namespace DnsResolver.Infrastructure.Data;

using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // 确保数据库已创建
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("数据库初始化完成");

            // 检查是否需要创建默认管理员
            if (!await context.Users.AnyAsync())
            {
                var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                var passwordHash = passwordHasher.Hash("admin123");
                var admin = new User(adminId, "admin", passwordHash, "admin");

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();

                logger.LogInformation("已创建默认管理员账号: admin / admin123");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }
}
