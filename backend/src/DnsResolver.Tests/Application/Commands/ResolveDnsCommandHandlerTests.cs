namespace DnsResolver.Tests.Application.Commands;

using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Domain.Aggregates.DnsQuery;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Services;
using DnsResolver.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ResolveDnsCommandHandlerTests
{
    private readonly Mock<IDnsResolutionService> _resolutionServiceMock;
    private readonly Mock<IIspProviderRepository> _ispRepositoryMock;
    private readonly Mock<ILogger<ResolveDnsCommandHandler>> _loggerMock;
    private readonly ResolveDnsCommandHandler _handler;

    public ResolveDnsCommandHandlerTests()
    {
        _resolutionServiceMock = new Mock<IDnsResolutionService>();
        _ispRepositoryMock = new Mock<IIspProviderRepository>();
        _loggerMock = new Mock<ILogger<ResolveDnsCommandHandler>>();
        _handler = new ResolveDnsCommandHandler(
            _resolutionServiceMock.Object,
            _ispRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new ResolveDnsCommand("example.com", "A", "8.8.8.8");
        var domain = DomainName.Create("example.com");
        var recordType = RecordType.Create("A");
        var dnsServer = DnsServer.Create("8.8.8.8");

        var dnsRecord = new DnsRecord("192.0.2.1", 300, RecordType.A);
        var resolveResult = ResolveResult.Succeeded(new[] { dnsRecord }, 50);
        var dnsQuery = DnsQuery.Create(domain, recordType, dnsServer, "Google DNS");
        dnsQuery.SetResult(new[] { dnsRecord }, 50);

        _ispRepositoryMock
            .Setup(x => x.FindByDnsServerAsync("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IspProvider.Create("google", "Google DNS", "8.8.8.8"));

        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(
                It.IsAny<DomainName>(),
                It.IsAny<RecordType>(),
                It.IsAny<DnsServer>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dnsQuery);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Domain.Should().Be("example.com");
        result.RecordType.Should().Be("A");
        result.DnsServer.Should().Be("8.8.8.8");
        result.IspName.Should().Be("Google DNS");
        result.Records.Should().HaveCount(1);
        result.Records[0].Value.Should().Be("192.0.2.1");
        result.Records[0].Ttl.Should().Be(300);
        result.QueryTimeMs.Should().Be(50);
    }

    [Fact]
    public async Task HandleAsync_WithCustomDns_ShouldReturnCustomDnsName()
    {
        // Arrange
        var command = new ResolveDnsCommand("example.com", "A", "1.2.3.4");
        var domain = DomainName.Create("example.com");
        var recordType = RecordType.Create("A");
        var dnsServer = DnsServer.Create("1.2.3.4");

        var dnsRecord = new DnsRecord("192.0.2.1", 300, RecordType.A);
        var dnsQuery = DnsQuery.Create(domain, recordType, dnsServer, null);
        dnsQuery.SetResult(new[] { dnsRecord }, 50);

        _ispRepositoryMock
            .Setup(x => x.FindByDnsServerAsync("1.2.3.4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IspProvider?)null);

        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(
                It.IsAny<DomainName>(),
                It.IsAny<RecordType>(),
                It.IsAny<DnsServer>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dnsQuery);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IspName.Should().Be("自定义DNS");
    }

    [Fact]
    public async Task HandleAsync_WithFailedResolution_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new ResolveDnsCommand("example.com", "A", "8.8.8.8");
        var domain = DomainName.Create("example.com");
        var recordType = RecordType.Create("A");
        var dnsServer = DnsServer.Create("8.8.8.8");

        var resolveResult = ResolveResult.Failed("DNS query timeout");
        var dnsQuery = DnsQuery.Create(domain, recordType, dnsServer, "Google DNS");
        dnsQuery.SetError("DNS query timeout");

        _ispRepositoryMock
            .Setup(x => x.FindByDnsServerAsync("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IspProvider.Create("google", "Google DNS", "8.8.8.8"));

        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(
                It.IsAny<DomainName>(),
                It.IsAny<RecordType>(),
                It.IsAny<DnsServer>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dnsQuery);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("DNS query timeout");
        result.Records.Should().BeEmpty();
        result.QueryTimeMs.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallResolutionServiceWithCorrectParameters()
    {
        // Arrange
        var command = new ResolveDnsCommand("example.com", "A", "8.8.8.8");
        var domain = DomainName.Create("example.com");
        var recordType = RecordType.Create("A");
        var dnsServer = DnsServer.Create("8.8.8.8");

        var dnsRecord = new DnsRecord("192.0.2.1", 300, RecordType.A);
        var resolveResult = ResolveResult.Succeeded(new[] { dnsRecord }, 50);
        var dnsQuery = DnsQuery.Create(domain, recordType, dnsServer, "Google DNS");
        dnsQuery.SetResult(new[] { dnsRecord }, 50);

        _ispRepositoryMock
            .Setup(x => x.FindByDnsServerAsync("8.8.8.8", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IspProvider.Create("google", "Google DNS", "8.8.8.8"));

        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(
                It.IsAny<DomainName>(),
                It.IsAny<RecordType>(),
                It.IsAny<DnsServer>(),
                "Google DNS",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dnsQuery);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _resolutionServiceMock.Verify(
            x => x.ResolveAsync(
                It.Is<DomainName>(d => d.Value == "example.com"),
                It.Is<RecordType>(r => r.Value == "A"),
                It.Is<DnsServer>(s => s.Address == "8.8.8.8"),
                "Google DNS",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
