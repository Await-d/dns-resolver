using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DnsResolver.Tests.Integration;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 可以在这里替换服务进行测试
            // 例如替换外部依赖为 Mock
        });
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
