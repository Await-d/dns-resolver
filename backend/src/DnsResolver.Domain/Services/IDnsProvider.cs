namespace DnsResolver.Domain.Services;

using DnsResolver.Domain.ValueObjects;

public interface IDnsProvider
{
    string Name { get; }
    string DisplayName { get; }
    DnsProviderFieldMeta FieldMeta { get; }

    void Configure(DnsProviderConfig config);

    Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default);

    Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default);

    Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
        string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default);

    Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
        string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default);

    Task<ProviderResult> DeleteRecordAsync(
        string domain, string recordId, CancellationToken ct = default);
}

public record DnsProviderConfig(string Id, string Secret, Dictionary<string, string>? ExtraParams = null);

public record DnsRecordInfo(
    string RecordId, string Domain, string SubDomain, string FullDomain,
    string RecordType, string Value, int Ttl, bool Enabled = true);

public enum ProviderErrorCode
{
    Success, AuthenticationFailed, DomainNotFound, RecordNotFound,
    RecordExists, RateLimited, InvalidParameter, NetworkError, UnknownError
}

public record ProviderResult(bool Success, ProviderErrorCode ErrorCode = ProviderErrorCode.Success, string? ErrorMessage = null)
{
    public static ProviderResult Ok() => new(true);
    public static ProviderResult Fail(ProviderErrorCode code, string message) => new(false, code, message);
}

public record ProviderResult<T>(bool Success, T? Data = default, ProviderErrorCode ErrorCode = ProviderErrorCode.Success, string? ErrorMessage = null)
{
    public static ProviderResult<T> Ok(T data) => new(true, data);
    public static ProviderResult<T> Fail(ProviderErrorCode code, string message) => new(false, default, code, message);
}
