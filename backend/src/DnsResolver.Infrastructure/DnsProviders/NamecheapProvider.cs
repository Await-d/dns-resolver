namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class NamecheapProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.namecheap.com/xml.response";

    public override string Name => "namecheap";
    public override string DisplayName => "Namecheap";

    public NamecheapProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        // Namecheap requires XML parsing, simplified implementation
        return await Task.FromResult(ProviderResult<IReadOnlyList<string>>.Fail(
            ProviderErrorCode.InvalidParameter, "Use Namecheap web interface to list domains"));
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        return await Task.FromResult(ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(
            ProviderErrorCode.InvalidParameter, "Use Namecheap web interface to list records"));
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
        string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        // Namecheap uses setHosts which replaces all records
        return await Task.FromResult(ProviderResult<DnsRecordInfo>.Fail(
            ProviderErrorCode.InvalidParameter, "Use Namecheap web interface to add records"));
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
        string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        return await Task.FromResult(ProviderResult<DnsRecordInfo>.Fail(
            ProviderErrorCode.InvalidParameter, "Use Namecheap web interface to update records"));
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        return await Task.FromResult(ProviderResult.Fail(
            ProviderErrorCode.InvalidParameter, "Use Namecheap web interface to delete records"));
    }
}
