namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class DnslaProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.dns.la/api";

    public override string Name => "dnsla";
    public override string DisplayName => "DNS.LA";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "APIID",
        SecretLabel: "API 密钥",
        HelpUrl: "https://console.dns.la/login?aksk=1"
    );

    public DnslaProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{config.Id}:{config.Secret}")));
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<DnslaDomainsResponse>($"{Endpoint}/domainList", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Data?.Select(d => d.Domain).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var domainId = await GetDomainIdAsync(domain, ct);
            if (domainId == null) return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.DomainNotFound, "Domain not found");

            var response = await HttpClient.GetFromJsonAsync<DnslaRecordsResponse>($"{Endpoint}/recordList?domainId={domainId}", JsonOptions, ct);
            var records = response?.Data?.Select(r => new DnsRecordInfo(r.Id, domain, r.Host, GetFullDomain(r.Host, domain), r.Type, r.Data, r.Ttl)).ToList() ?? [];
            if (!string.IsNullOrEmpty(subDomain)) records = records.Where(r => r.SubDomain == subDomain).ToList();
            if (!string.IsNullOrEmpty(recordType)) records = records.Where(r => r.RecordType == recordType).ToList();
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var domainId = await GetDomainIdAsync(domain, ct);
            if (domainId == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Domain not found");

            var body = new { domainId, host = subDomain, type = recordType, data = value, ttl };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/record", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<DnslaRecordResponse>(JsonOptions, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(result?.Data?.Id ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var body = new { id = recordId, data = value, ttl = ttl ?? 600 };
            await HttpClient.PutAsJsonAsync($"{Endpoint}/record", body, JsonOptions, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 600));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try { await HttpClient.DeleteAsync($"{Endpoint}/record?id={recordId}", ct); return ProviderResult.Ok(); }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private async Task<string?> GetDomainIdAsync(string domain, CancellationToken ct)
    {
        var response = await HttpClient.GetFromJsonAsync<DnslaDomainsResponse>($"{Endpoint}/domainList", JsonOptions, ct);
        return response?.Data?.FirstOrDefault(d => d.Domain == domain)?.Id;
    }

    private class DnslaDomainsResponse { public List<DnslaDomain>? Data { get; set; } }
    private class DnslaDomain { public string Id { get; set; } = ""; public string Domain { get; set; } = ""; }
    private class DnslaRecordsResponse { public List<DnslaRecord>? Data { get; set; } }
    private class DnslaRecord { public string Id { get; set; } = ""; public string Host { get; set; } = ""; public string Type { get; set; } = ""; public string Data { get; set; } = ""; public int Ttl { get; set; } }
    private class DnslaRecordResponse { public DnslaRecordId? Data { get; set; } }
    private class DnslaRecordId { public string Id { get; set; } = ""; }
}
