namespace DnsResolver.Api.Requests;

public record ConfigureProviderRequest(
    string ProviderName,
    string Id,
    string Secret,
    Dictionary<string, string>? ExtraParams = null);

public record AddRecordRequest(
    string ProviderName,
    string Domain,
    string SubDomain,
    string RecordType,
    string Value,
    int Ttl = 600);

public record UpdateRecordRequest(
    string ProviderName,
    string Domain,
    string RecordId,
    string Value,
    int? Ttl = null);

public record DeleteRecordRequest(
    string ProviderName,
    string Domain,
    string RecordId);

public record GetRecordsRequest(
    string ProviderName,
    string Domain,
    string? SubDomain = null,
    string? RecordType = null);
