namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class DnspodProvider : BaseDnsProvider
{
    private const string Endpoint = "https://dnsapi.cn";

    public override string Name => "dnspod";
    public override string DisplayName => "DNSPod";

    public DnspodProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await PostAsync<DnspodDomainsResponse>("Domain.List", new(), ct);
            if (result?.Status?.Code != "1")
                return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.UnknownError, result?.Status?.Message ?? "Failed");
            return ProviderResult<IReadOnlyList<string>>.Ok(result.Domains?.Select(d => d.Name).ToList() ?? []);
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
            var @params = new Dictionary<string, string> { ["domain"] = domain };
            if (!string.IsNullOrEmpty(subDomain)) @params["sub_domain"] = subDomain;
            if (!string.IsNullOrEmpty(recordType)) @params["record_type"] = recordType;

            var result = await PostAsync<DnspodRecordsResponse>("Record.List", @params, ct);
            if (result?.Status?.Code != "1")
                return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.UnknownError, result?.Status?.Message ?? "Failed");

            var records = result.Records?.Select(r => new DnsRecordInfo(
                r.Id, domain, r.Name, GetFullDomain(r.Name, domain), r.Type, r.Value, int.TryParse(r.Ttl, out var ttl) ? ttl : 600
            )).ToList() ?? [];

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
            var @params = new Dictionary<string, string>
            {
                ["domain"] = domain, ["sub_domain"] = subDomain, ["record_type"] = recordType,
                ["value"] = value, ["ttl"] = ttl.ToString(), ["record_line"] = "默认"
            };
            var result = await PostAsync<DnspodRecordResponse>("Record.Create", @params, ct);
            if (result?.Status?.Code != "1")
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, result?.Status?.Message ?? "Failed");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                result.Record?.Id ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
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

            var @params = new Dictionary<string, string>
            {
                ["domain"] = domain, ["record_id"] = recordId, ["sub_domain"] = existing.SubDomain,
                ["record_type"] = existing.RecordType, ["value"] = value, ["ttl"] = (ttl ?? existing.Ttl).ToString(), ["record_line"] = "默认"
            };
            var result = await PostAsync<DnspodRecordResponse>("Record.Modify", @params, ct);
            if (result?.Status?.Code != "1")
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, result?.Status?.Message ?? "Failed");

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
            var result = await PostAsync<DnspodStatusResponse>("Record.Remove", new() { ["domain"] = domain, ["record_id"] = recordId }, ct);
            return result?.Status?.Code == "1" ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, result?.Status?.Message ?? "Failed");
        }
        catch (Exception ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private async Task<T?> PostAsync<T>(string action, Dictionary<string, string> @params, CancellationToken ct)
    {
        @params["login_token"] = $"{Config.Id},{Config.Secret}";
        @params["format"] = "json";
        var content = new FormUrlEncodedContent(@params);
        var response = await HttpClient.PostAsync($"{Endpoint}/{action}", content, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private class DnspodStatusResponse { public DnspodStatus? Status { get; set; } }
    private class DnspodStatus { public string Code { get; set; } = ""; public string? Message { get; set; } }
    private class DnspodDomainsResponse : DnspodStatusResponse { public List<DnspodDomain>? Domains { get; set; } }
    private class DnspodDomain { public string Name { get; set; } = ""; }
    private class DnspodRecordsResponse : DnspodStatusResponse { public List<DnspodRecord>? Records { get; set; } }
    private class DnspodRecord { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public string Ttl { get; set; } = "600"; }
    private class DnspodRecordResponse : DnspodStatusResponse { public DnspodRecordId? Record { get; set; } }
    private class DnspodRecordId { public string Id { get; set; } = ""; }
}
