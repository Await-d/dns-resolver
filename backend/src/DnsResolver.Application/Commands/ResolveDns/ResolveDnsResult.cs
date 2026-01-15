namespace DnsResolver.Application.Commands.ResolveDns;

using DnsResolver.Application.DTOs;

public record ResolveDnsResult(
    string Domain,
    string RecordType,
    string DnsServer,
    string IspName,
    IReadOnlyList<DnsRecordDto> Records,
    int QueryTimeMs,
    bool Success,
    string? ErrorMessage
);
