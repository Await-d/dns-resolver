using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace DnsResolver.Tests.Integration;

public class DnsControllerIntegrationTests : IntegrationTestBase
{
    public DnsControllerIntegrationTests(WebApplicationFactoryFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetIsps_ReturnsSuccessAndIspList()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/dns/isps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("telecom");
        content.Should().Contain("unicom");
        content.Should().Contain("mobile");
    }

    [Fact]
    public async Task Resolve_WithValidDomain_ReturnsSuccess()
    {
        // Arrange
        var request = new
        {
            domain = "example.com",
            recordType = "A",
            dnsServer = "8.8.8.8"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/dns/resolve", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("success");
    }

    [Fact]
    public async Task Resolve_WithInvalidDomain_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            domain = "",
            recordType = "A",
            dnsServer = "8.8.8.8"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/dns/resolve", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Compare_WithValidRequest_ReturnsComparisonResults()
    {
        // Arrange
        var request = new
        {
            domain = "example.com",
            recordType = "A",
            ispList = new[] { "google", "cloudflare" }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/dns/compare", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("success");
    }
}
