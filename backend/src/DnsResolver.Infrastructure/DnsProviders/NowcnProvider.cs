namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class NowcnProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.now.cn";

    public override string Name => "nowcn";
    public override string DisplayName => "时代互联";

    public NowcnProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.InvalidParameter, "Use web interface to list domains"));

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.InvalidParameter, "Use web interface to list records"));

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var url = $"{Endpoint}/domain/dns?username={Config.Id}&password={Config.Secret}&domain={domain}&host={subDomain}&type={recordType}&value={Uri.EscapeDataString(value)}&ttl={ttl}&act=add";
            var response = await HttpClient.GetStringAsync(url, ct);
            return response.Contains("success") ? ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo($"{subDomain}_{recordType}", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl))
                : ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, response);
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        var parts = recordId.Split('_', 2);
        return await AddRecordAsync(domain, parts[0], parts.Length > 1 ? parts[1] : "A", value, ttl ?? 600, ct);
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var parts = recordId.Split('_', 2);
            var url = $"{Endpoint}/domain/dns?username={Config.Id}&password={Config.Secret}&domain={domain}&host={parts[0]}&type={parts[1]}&act=del";
            await HttpClient.GetStringAsync(url, ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }
}
