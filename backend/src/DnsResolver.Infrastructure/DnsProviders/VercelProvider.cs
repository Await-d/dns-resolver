namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class VercelProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.vercel.com";

    public override string Name => "vercel";
    public override string DisplayName => "Vercel";

    public VercelProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<VcDomainsResponse>($"{Endpoint}/v5/domains", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Domains?.Select(d => d.Name).ToList() ?? []);
        }
        catch (Exception ex)
        {
            return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<VcRecordsResponse>($"{Endpoint}/v4/domains/{domain}/records", JsonOptions, ct);
            var records = response?.Records?.Select(r => new DnsRecordInfo(
                r.Id, domain, r.Name, GetFullDomain(r.Name, domain), r.Type, r.Value, r.Ttl ?? 60
            )).ToList() ?? [];

            if (!string.IsNullOrEmpty(subDomain)) records = records.Where(r => r.SubDomain == subDomain).ToList();
            if (!string.IsNullOrEmpty(recordType)) records = records.Where(r => r.RecordType == recordType).ToList();

            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (Exception ex)
        {
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
        string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var body = new { name = subDomain, type = recordType, value, ttl };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/v2/domains/{domain}/records", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<VcRecordResponse>(JsonOptions, ct);

            if (string.IsNullOrEmpty(result?.Uid))
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed to create record");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                result.Uid, domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
        string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var getResult = await GetRecordsAsync(domain, ct: ct);
            var existing = getResult.Data?.FirstOrDefault(r => r.RecordId == recordId);
            if (existing == null)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.RecordNotFound, "Record not found");

            var body = new { value, ttl = ttl ?? existing.Ttl };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{Endpoint}/v1/domains/records/{recordId}")
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            var response = await HttpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));

            return ProviderResult<DnsRecordInfo>.Ok(existing with { Value = value, Ttl = ttl ?? existing.Ttl });
        }
        catch (Exception ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"{Endpoint}/v2/domains/{domain}/records/{recordId}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (Exception ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private class VcDomainsResponse { public List<VcDomain>? Domains { get; set; } }
    private class VcDomain { public string Name { get; set; } = ""; }
    private class VcRecordsResponse { public List<VcRecord>? Records { get; set; } }
    private class VcRecord { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public int? Ttl { get; set; } }
    private class VcRecordResponse { public string? Uid { get; set; } }
}
