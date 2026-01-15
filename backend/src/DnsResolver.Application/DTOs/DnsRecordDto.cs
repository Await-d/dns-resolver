namespace DnsResolver.Application.DTOs;

public record DnsRecordDto(string Value, int Ttl, string RecordType);
