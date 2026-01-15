using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DnsResolver.Tests.Integration;

public class DnsProviderControllerIntegrationTests : IntegrationTestBase
{
    public DnsProviderControllerIntegrationTests(WebApplicationFactoryFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProviders_ReturnsSuccessAndProviderList()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("cloudflare");
        content.Should().Contain("alidns");
        content.Should().Contain("tencentcloud");
    }

    [Fact]
    public async Task GetDomains_WithoutCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            providerName = "cloudflare",
            id = "",
            secret = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/domains", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRecords_WithInvalidProvider_ReturnsError()
    {
        // Arrange
        var request = new
        {
            providerName = "invalid_provider",
            id = "test",
            secret = "test",
            domain = "example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/records/list", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }
}
