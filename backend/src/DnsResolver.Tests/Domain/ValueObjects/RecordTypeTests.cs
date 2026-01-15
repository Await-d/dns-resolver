namespace DnsResolver.Tests.Domain.ValueObjects;

using DnsResolver.Domain.Exceptions;
using DnsResolver.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

public class RecordTypeTests
{
    [Theory]
    [InlineData("A")]
    [InlineData("AAAA")]
    [InlineData("CNAME")]
    [InlineData("MX")]
    [InlineData("TXT")]
    [InlineData("NS")]
    [InlineData("SOA")]
    public void Create_WithValidType_ShouldSucceed(string type)
    {
        // Act
        var result = RecordType.Create(type);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(type.ToUpperInvariant());
    }

    [Theory]
    [InlineData("a", "A")]
    [InlineData("aaaa", "AAAA")]
    [InlineData("cname", "CNAME")]
    [InlineData("  A  ", "A")]
    [InlineData("Mx", "MX")]
    public void Create_ShouldNormalizeType(string input, string expected)
    {
        // Act
        var result = RecordType.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyType_ShouldThrowDomainException(string type)
    {
        // Act
        Action act = () => RecordType.Create(type);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*不支持的记录类型*");
    }

    [Fact]
    public void Create_WithNullType_ShouldThrowDomainException()
    {
        // Act
        Action act = () => RecordType.Create(null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("记录类型不能为空");
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("PTR")]
    [InlineData("SRV")]
    [InlineData("CAA")]
    public void Create_WithUnsupportedType_ShouldThrowDomainException(string type)
    {
        // Act
        Action act = () => RecordType.Create(type);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"不支持的记录类型: {type}");
    }

    [Fact]
    public void StaticProperties_ShouldBeInitialized()
    {
        // Assert
        RecordType.A.Value.Should().Be("A");
        RecordType.AAAA.Value.Should().Be("AAAA");
        RecordType.CNAME.Value.Should().Be("CNAME");
        RecordType.MX.Value.Should().Be("MX");
        RecordType.TXT.Value.Should().Be("TXT");
        RecordType.NS.Value.Should().Be("NS");
        RecordType.SOA.Value.Should().Be("SOA");
    }

    [Fact]
    public void Equals_WithSameType_ShouldReturnTrue()
    {
        // Arrange
        var type1 = RecordType.Create("A");
        var type2 = RecordType.Create("A");

        // Act & Assert
        type1.Equals(type2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var type1 = RecordType.Create("A");
        var type2 = RecordType.Create("AAAA");

        // Act & Assert
        type1.Equals(type2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var type = RecordType.Create("A");

        // Act
        var result = type.ToString();

        // Assert
        result.Should().Be("A");
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var type1 = RecordType.Create("A");
        var type2 = RecordType.Create("A");

        // Act & Assert
        type1.GetHashCode().Should().Be(type2.GetHashCode());
    }
}
