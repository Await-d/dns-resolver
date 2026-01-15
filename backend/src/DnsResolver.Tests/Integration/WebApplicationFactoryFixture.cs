using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DnsResolver.Infrastructure.Data;
using Xunit;

namespace DnsResolver.Tests.Integration;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 移除所有 DbContext 相关注册
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                           d.ServiceType == typeof(AppDbContext) ||
                           d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // 使用内存数据库进行测试
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // 确保数据库被创建并初始化
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return host;
    }
}

public class IntegrationTestBase : IClassFixture<WebApplicationFactoryFixture>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactoryFixture Factory;

    public IntegrationTestBase(WebApplicationFactoryFixture factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
