namespace DnsResolver.Tests.DnsProviders;

using System.Net;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.DnsProviders;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

public class BaseDnsProviderTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly TestDnsProvider _provider;

    public BaseDnsProviderTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _provider = new TestDnsProvider(_httpClient);
    }

    [Fact]
    public void Configure_WithValidConfig_ShouldSetConfig()
    {
        // Arrange
        var config = new DnsProviderConfig("test-id", "test-secret");

        // Act
        _provider.Configure(config);

        // Assert
        _provider.GetConfig().Should().Be(config);
        _provider.GetConfig().Id.Should().Be("test-id");
        _provider.GetConfig().Secret.Should().Be("test-secret");
    }

    [Fact]
    public void Configure_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _provider.Configure(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Assert
        _provider.Name.Should().Be("test");
    }

    [Fact]
    public void DisplayName_ShouldReturnProviderDisplayName()
    {
        // Assert
        _provider.DisplayName.Should().Be("Test Provider");
    }

    private class TestDnsProvider : BaseDnsProvider
    {
        public TestDnsProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "test";
        public override string DisplayName => "Test Provider";

        public DnsProviderConfig GetConfig() => Config;

        public Task<T?> TestGetJsonAsync<T>(string url, CancellationToken ct = default) =>
            GetJsonAsync<T>(url, ct);

        public Task<TResponse?> TestPostJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default) =>
            PostJsonAsync<TRequest, TResponse>(url, data, ct);

        public override Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default) =>
            Task.FromResult(ProviderResult<IReadOnlyList<string>>.Ok(new List<string>()));

        public override Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
            string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default) =>
            Task.FromResult(ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(new List<DnsRecordInfo>()));

        public override Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
            string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default) =>
            Task.FromResult(ProviderResult<DnsRecordInfo>.Ok(
                new DnsRecordInfo("1", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl)));

        public override Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
            string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default) =>
            Task.FromResult(ProviderResult<DnsRecordInfo>.Ok(
                new DnsRecordInfo(recordId, domain, "www", "www." + domain, "A", value, ttl ?? 600)));

        public override Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default) =>
            Task.FromResult(ProviderResult.Ok());
    }

    private class TestRequest
    {
        public string Name { get; set; } = "";
    }

    private class TestResponse
    {
        public string Message { get; set; } = "";
        public int Code { get; set; }
    }
}
