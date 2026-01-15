namespace DnsResolver.Domain.ValueObjects;

using DnsResolver.Domain.Exceptions;

public sealed class RecordType : IEquatable<RecordType>
{
    public static readonly RecordType A = new("A");
    public static readonly RecordType AAAA = new("AAAA");
    public static readonly RecordType CNAME = new("CNAME");
    public static readonly RecordType MX = new("MX");
    public static readonly RecordType TXT = new("TXT");
    public static readonly RecordType NS = new("NS");
    public static readonly RecordType SOA = new("SOA");

    private static readonly HashSet<string> ValidTypes = ["A", "AAAA", "CNAME", "MX", "TXT", "NS", "SOA"];

    public string Value { get; }

    private RecordType(string value) => Value = value;

    public static RecordType Create(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant()
            ?? throw new DomainException("记录类型不能为空");

        if (!ValidTypes.Contains(normalized))
            throw new DomainException($"不支持的记录类型: {value}");

        return new RecordType(normalized);
    }

    public bool Equals(RecordType? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is RecordType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
