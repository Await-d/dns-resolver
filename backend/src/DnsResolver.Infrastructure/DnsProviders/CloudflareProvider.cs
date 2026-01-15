namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class CloudflareProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.cloudflare.com/client/v4";

    public override string Name => "cloudflare";
    public override string DisplayName => "Cloudflare";

    public CloudflareProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<CfZonesResponse>($"{Endpoint}/zones", JsonOptions, ct);
            if (response?.Success != true)
                return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.UnknownError, "Failed to get zones");
            return ProviderResult<IReadOnlyList<string>>.Ok(response.Result?.Select(z => z.Name).ToList() ?? []);
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null)
                return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var url = $"{Endpoint}/zones/{zoneId}/dns_records";
            if (!string.IsNullOrEmpty(recordType)) url += $"?type={recordType}";

            var response = await HttpClient.GetFromJsonAsync<CfRecordsResponse>(url, JsonOptions, ct);
            if (response?.Success != true)
                return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.UnknownError, "Failed to get records");

            var records = response.Result?.Select(r => new DnsRecordInfo(
                r.Id, domain, r.Name == domain ? "@" : r.Name.Replace($".{domain}", ""),
                r.Name, r.Type, r.Content, r.Ttl
            )).ToList() ?? [];

            if (!string.IsNullOrEmpty(subDomain))
                records = records.Where(r => r.SubDomain == subDomain).ToList();

            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
        string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var fullDomain = GetFullDomain(subDomain, domain);
            var body = new { type = recordType, name = fullDomain, content = value, ttl };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/zones/{zoneId}/dns_records", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<CfRecordResponse>(JsonOptions, ct);

            if (result?.Success != true)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, result?.Errors?.FirstOrDefault()?.Message ?? "Failed");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                result.Result!.Id, domain, subDomain, fullDomain, recordType, value, ttl));
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
        string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var getResult = await GetRecordsAsync(domain, ct: ct);
            var existing = getResult.Data?.FirstOrDefault(r => r.RecordId == recordId);
            if (existing == null)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.RecordNotFound, "Record not found");

            var body = new { type = existing.RecordType, name = existing.FullDomain, content = value, ttl = ttl ?? existing.Ttl };
            var response = await HttpClient.PutAsJsonAsync($"{Endpoint}/zones/{zoneId}/dns_records/{recordId}", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<CfRecordResponse>(JsonOptions, ct);

            if (result?.Success != true)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed to update");

            return ProviderResult<DnsRecordInfo>.Ok(existing with { Value = value, Ttl = ttl ?? existing.Ttl });
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null)
                return ProviderResult.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var response = await HttpClient.DeleteAsync($"{Endpoint}/zones/{zoneId}/dns_records/{recordId}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private async Task<string?> GetZoneIdAsync(string domain, CancellationToken ct)
    {
        var response = await HttpClient.GetFromJsonAsync<CfZonesResponse>($"{Endpoint}/zones?name={domain}", JsonOptions, ct);
        return response?.Result?.FirstOrDefault()?.Id;
    }

    private class CfZonesResponse { public bool Success { get; set; } public List<CfZone>? Result { get; set; } }
    private class CfZone { public string Id { get; set; } = ""; public string Name { get; set; } = ""; }
    private class CfRecordsResponse { public bool Success { get; set; } public List<CfRecord>? Result { get; set; } }
    private class CfRecord { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Content { get; set; } = ""; public int Ttl { get; set; } }
    private class CfRecordResponse { public bool Success { get; set; } public CfRecord? Result { get; set; } public List<CfError>? Errors { get; set; } }
    private class CfError { public string? Message { get; set; } }
}
