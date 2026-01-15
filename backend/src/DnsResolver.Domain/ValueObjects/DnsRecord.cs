namespace DnsResolver.Domain.ValueObjects;

public sealed record DnsRecord(string Value, int Ttl, RecordType Type);
