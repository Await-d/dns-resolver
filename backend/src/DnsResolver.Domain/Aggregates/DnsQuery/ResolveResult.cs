namespace DnsResolver.Domain.Aggregates.DnsQuery;

using DnsResolver.Domain.ValueObjects;

public class ResolveResult
{
    public IReadOnlyList<DnsRecord> Records { get; }
    public int QueryTimeMs { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private ResolveResult(IReadOnlyList<DnsRecord> records, int queryTimeMs, bool success, string? errorMessage)
    {
        Records = records;
        QueryTimeMs = queryTimeMs;
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static ResolveResult Succeeded(IReadOnlyList<DnsRecord> records, int queryTimeMs) =>
        new(records, queryTimeMs, true, null);

    public static ResolveResult Failed(string errorMessage) =>
        new(Array.Empty<DnsRecord>(), 0, false, errorMessage);
}
