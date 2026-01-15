namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class Dynv6Provider : BaseDnsProvider
{
    private const string Endpoint = "https://dynv6.com/api/v2";

    public override string Name => "dynv6";
    public override string DisplayName => "Dynv6";

    public Dynv6Provider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<List<Dv6Zone>>($"{Endpoint}/zones", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Select(z => z.Name).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var response = await HttpClient.GetFromJsonAsync<List<Dv6Record>>($"{Endpoint}/zones/{zoneId}/records", JsonOptions, ct);
            var records = response?.Select(r => new DnsRecordInfo(r.Id.ToString(), domain, r.Name, GetFullDomain(r.Name, domain), r.Type, r.Data, 60)).ToList() ?? [];
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
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var body = new { name = subDomain, type = recordType, data = value };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/zones/{zoneId}/records", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<Dv6Record>(JsonOptions, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(result?.Id.ToString() ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var body = new { data = value };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{Endpoint}/zones/{zoneId}/records/{recordId}") { Content = JsonContent.Create(body, options: JsonOptions) };
            await HttpClient.SendAsync(request, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 60));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");
            await HttpClient.DeleteAsync($"{Endpoint}/zones/{zoneId}/records/{recordId}", ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private async Task<long?> GetZoneIdAsync(string domain, CancellationToken ct)
    {
        var zones = await HttpClient.GetFromJsonAsync<List<Dv6Zone>>($"{Endpoint}/zones", JsonOptions, ct);
        return zones?.FirstOrDefault(z => z.Name == domain)?.Id;
    }

    private class Dv6Zone { public long Id { get; set; } public string Name { get; set; } = ""; }
    private class Dv6Record { public long Id { get; set; } public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Data { get; set; } = ""; }
}
