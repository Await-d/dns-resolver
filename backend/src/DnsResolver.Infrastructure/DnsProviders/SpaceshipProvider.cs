namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class SpaceshipProvider : BaseDnsProvider
{
    private const string Endpoint = "https://spaceship.dev/api/v1";

    public override string Name => "spaceship";
    public override string DisplayName => "Spaceship";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "API Key",
        SecretLabel: "API Secret",
        HelpUrl: "https://www.spaceship.com/application/api-manager/"
    );

    public SpaceshipProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Add("X-Api-Key", config.Id);
        HttpClient.DefaultRequestHeaders.Add("X-Api-Secret", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<SpDomainsResponse>($"{Endpoint}/domains", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Items?.Select(d => d.Name).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<SpRecordsResponse>($"{Endpoint}/dns/records?domain={domain}", JsonOptions, ct);
            var records = response?.Items?.Select(r => new DnsRecordInfo(r.Id, domain, r.Host, GetFullDomain(r.Host, domain), r.Type, r.Value, r.Ttl)).ToList() ?? [];
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
            var body = new { domain, host = subDomain, type = recordType, value, ttl };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/dns/records", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<SpRecord>(JsonOptions, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(result?.Id ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var body = new { value, ttl = ttl ?? 600 };
            var response = await HttpClient.PutAsJsonAsync($"{Endpoint}/dns/records/{recordId}", body, JsonOptions, ct);
            return response.IsSuccessStatusCode ? ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 600))
                : ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"{Endpoint}/dns/records/{recordId}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private class SpDomainsResponse { public List<SpDomain>? Items { get; set; } }
    private class SpDomain { public string Name { get; set; } = ""; }
    private class SpRecordsResponse { public List<SpRecord>? Items { get; set; } }
    private class SpRecord { public string Id { get; set; } = ""; public string Host { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public int Ttl { get; set; } }
}
