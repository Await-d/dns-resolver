namespace DnsResolver.Application.Commands.CompareDns;

using DnsResolver.Application.Commands.ResolveDns;

public record CompareDnsResult(
    string Domain,
    string RecordType,
    IReadOnlyList<ResolveDnsResult> Results
);
