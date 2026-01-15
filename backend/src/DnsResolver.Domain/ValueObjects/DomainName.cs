namespace DnsResolver.Domain.ValueObjects;

using System.Text.RegularExpressions;
using DnsResolver.Domain.Exceptions;

public sealed class DomainName : IEquatable<DomainName>
{
    public string Value { get; }

    private DomainName(string value) => Value = value;

    public static DomainName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDomainException("域名不能为空");

        var normalized = value.Trim().ToLowerInvariant();

        if (!IsValidDomain(normalized))
            throw new InvalidDomainException($"无效的域名格式: {value}");

        return new DomainName(normalized);
    }

    private static bool IsValidDomain(string domain)
    {
        var pattern = @"^[a-z0-9]([a-z0-9\-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9\-]{0,61}[a-z0-9])?)*\.[a-z]{2,}$";
        return Regex.IsMatch(domain, pattern);
    }

    public bool Equals(DomainName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is DomainName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(DomainName domain) => domain.Value;
}
