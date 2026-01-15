namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class GodaddyProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.godaddy.com/v1";

    public override string Name => "godaddy";
    public override string DisplayName => "GoDaddy";

    public GodaddyProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("sso-key", $"{config.Id}:{config.Secret}");
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<List<GdDomain>>($"{Endpoint}/domains", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Select(d => d.Domain).ToList() ?? []);
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
            var url = $"{Endpoint}/domains/{domain}/records";
            if (!string.IsNullOrEmpty(recordType)) url += $"/{recordType}";
            if (!string.IsNullOrEmpty(subDomain)) url += $"/{subDomain}";

            var response = await HttpClient.GetFromJsonAsync<List<GdRecord>>(url, JsonOptions, ct);
            var records = response?.Select(r => new DnsRecordInfo(
                $"{r.Name}_{r.Type}", domain, r.Name, GetFullDomain(r.Name, domain), r.Type, r.Data, r.Ttl
            )).ToList() ?? [];

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
            var body = new[] { new { type = recordType, name = subDomain, data = value, ttl } };
            var response = await HttpClient.PatchAsJsonAsync($"{Endpoint}/domains/{domain}/records", body, JsonOptions, ct);

            if (!response.IsSuccessStatusCode)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                $"{subDomain}_{recordType}", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
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
            var parts = recordId.Split('_', 2);
            if (parts.Length != 2)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.InvalidParameter, "Invalid record ID");

            var subDomain = parts[0];
            var recordType = parts[1];
            var body = new[] { new { data = value, ttl = ttl ?? 600 } };
            var response = await HttpClient.PutAsJsonAsync($"{Endpoint}/domains/{domain}/records/{recordType}/{subDomain}", body, JsonOptions, ct);

            if (!response.IsSuccessStatusCode)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                recordId, domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl ?? 600));
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
            var parts = recordId.Split('_', 2);
            if (parts.Length != 2)
                return ProviderResult.Fail(ProviderErrorCode.InvalidParameter, "Invalid record ID");

            var response = await HttpClient.DeleteAsync($"{Endpoint}/domains/{domain}/records/{parts[1]}/{parts[0]}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (HttpRequestException ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private class GdDomain { public string Domain { get; set; } = ""; }
    private class GdRecord { public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Data { get; set; } = ""; public int Ttl { get; set; } }
}
