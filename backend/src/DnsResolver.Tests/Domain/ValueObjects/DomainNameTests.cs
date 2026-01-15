namespace DnsResolver.Tests.Domain.ValueObjects;

using DnsResolver.Domain.Exceptions;
using DnsResolver.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class DomainNameTests
{
    [Theory]
    [InlineData("example.com")]
    [InlineData("sub.example.com")]
    [InlineData("deep.sub.example.com")]
    [InlineData("example-domain.com")]
    [InlineData("123.example.com")]
    public void Create_WithValidDomain_ShouldSucceed(string domain)
    {
        // Act
        var result = DomainName.Create(domain);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(domain.ToLowerInvariant());
    }

    [Theory]
    [InlineData("EXAMPLE.COM", "example.com")]
    [InlineData("Example.Com", "example.com")]
    [InlineData("  example.com  ", "example.com")]
    public void Create_ShouldNormalizeDomain(string input, string expected)
    {
        // Act
        var result = DomainName.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyDomain_ShouldThrowInvalidDomainException(string? domain)
    {
        // Act
        Action act = () => DomainName.Create(domain!);

        // Assert
        act.Should().Throw<InvalidDomainException>()
            .WithMessage("域名不能为空");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid..com")]
    [InlineData("-invalid.com")]
    [InlineData("invalid-.com")]
    [InlineData("invalid_domain.com")]
    [InlineData("invalid domain.com")]
    [InlineData("192.168.1.1")]
    public void Create_WithInvalidDomain_ShouldThrowInvalidDomainException(string domain)
    {
        // Act
        Action act = () => DomainName.Create(domain);

        // Assert
        act.Should().Throw<InvalidDomainException>()
            .WithMessage($"无效的域名格式: {domain}");
    }

    [Fact]
    public void Equals_WithSameDomain_ShouldReturnTrue()
    {
        // Arrange
        var domain1 = DomainName.Create("example.com");
        var domain2 = DomainName.Create("example.com");

        // Act & Assert
        domain1.Equals(domain2).Should().BeTrue();
        (domain1 == domain2).Should().BeFalse(); // Different instances
    }

    [Fact]
    public void Equals_WithDifferentDomain_ShouldReturnFalse()
    {
        // Arrange
        var domain1 = DomainName.Create("example.com");
        var domain2 = DomainName.Create("other.com");

        // Act & Assert
        domain1.Equals(domain2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var domain = DomainName.Create("example.com");

        // Act
        var result = domain.ToString();

        // Assert
        result.Should().Be("example.com");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var domain = DomainName.Create("example.com");

        // Act
        string result = domain;

        // Assert
        result.Should().Be("example.com");
    }
}
