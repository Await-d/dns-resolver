namespace DnsResolver.Tests.Domain.ValueObjects;

using DnsResolver.Domain.Exceptions;
using DnsResolver.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class DnsServerTests
{
    [Theory]
    [InlineData("8.8.8.8", 53)]
    [InlineData("1.1.1.1", 53)]
    [InlineData("114.114.114.114", 53)]
    [InlineData("8.8.8.8", 5353)]
    public void Create_WithValidAddressAndPort_ShouldSucceed(string address, int port)
    {
        // Act
        var result = DnsServer.Create(address, port);

        // Assert
        result.Should().NotBeNull();
        result.Address.Should().Be(address);
        result.Port.Should().Be(port);
    }

    [Fact]
    public void Create_WithDefaultPort_ShouldUse53()
    {
        // Act
        var result = DnsServer.Create("8.8.8.8");

        // Assert
        result.Port.Should().Be(53);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("256.256.256.256")]
    [InlineData("")]
    [InlineData("example.com")]
    public void Create_WithInvalidAddress_ShouldThrowDomainException(string address)
    {
        // Act
        Action act = () => DnsServer.Create(address);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"无效的 DNS 服务器地址: {address}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void Create_WithInvalidPort_ShouldThrowDomainException(int port)
    {
        // Act
        Action act = () => DnsServer.Create("8.8.8.8", port);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"无效的端口号: {port}");
    }

    [Fact]
    public void Equals_WithSameAddressAndPort_ShouldReturnTrue()
    {
        // Arrange
        var server1 = DnsServer.Create("8.8.8.8", 53);
        var server2 = DnsServer.Create("8.8.8.8", 53);

        // Act & Assert
        server1.Equals(server2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAddress_ShouldReturnFalse()
    {
        // Arrange
        var server1 = DnsServer.Create("8.8.8.8", 53);
        var server2 = DnsServer.Create("1.1.1.1", 53);

        // Act & Assert
        server1.Equals(server2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentPort_ShouldReturnFalse()
    {
        // Arrange
        var server1 = DnsServer.Create("8.8.8.8", 53);
        var server2 = DnsServer.Create("8.8.8.8", 5353);

        // Act & Assert
        server1.Equals(server2).Should().BeFalse();
    }

    [Fact]
    public void ToString_WithDefaultPort_ShouldReturnAddressOnly()
    {
        // Arrange
        var server = DnsServer.Create("8.8.8.8", 53);

        // Act
        var result = server.ToString();

        // Assert
        result.Should().Be("8.8.8.8");
    }

    [Fact]
    public void ToString_WithCustomPort_ShouldReturnAddressAndPort()
    {
        // Arrange
        var server = DnsServer.Create("8.8.8.8", 5353);

        // Act
        var result = server.ToString();

        // Assert
        result.Should().Be("8.8.8.8:5353");
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var server1 = DnsServer.Create("8.8.8.8", 53);
        var server2 = DnsServer.Create("8.8.8.8", 53);

        // Act & Assert
        server1.GetHashCode().Should().Be(server2.GetHashCode());
    }
}
