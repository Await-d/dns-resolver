using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DnsResolver.Tests.Integration;

public class DdnsControllerIntegrationTests : IntegrationTestBase
{
    public DdnsControllerIntegrationTests(WebApplicationFactoryFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPublicIp_ReturnsSuccessAndIpAddress()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/ddns/ip?ipType=IPv4");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("success");
    }

    [Fact]
    public async Task UpdateDdns_WithoutCredentials_ReturnsError()
    {
        // Arrange
        var request = new
        {
            providerName = "cloudflare",
            id = "",
            secret = "",
            domain = "example.com",
            subDomain = "test",
            recordType = "A",
            ipType = "IPv4",
            forceUpdate = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ddns/update", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}
