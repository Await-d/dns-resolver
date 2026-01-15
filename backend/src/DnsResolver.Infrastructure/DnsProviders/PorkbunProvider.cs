namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class PorkbunProvider : BaseDnsProvider
{
    private const string Endpoint = "https://porkbun.com/api/json/v3";

    public override string Name => "porkbun";
    public override string DisplayName => "Porkbun";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "API Key",
        SecretLabel: "Secret Key",
        HelpUrl: "https://porkbun.com/account/api"
    );

    public PorkbunProvider(HttpClient httpClient) : base(httpClient) { }

    private object AuthBody => new { apikey = Config.Id, secretapikey = Config.Secret };

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/domain/listAll", AuthBody, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<PbDomainsResponse>(JsonOptions, ct);
            if (result?.Status != "SUCCESS")
                return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.UnknownError, result?.Message ?? "Failed");
            return ProviderResult<IReadOnlyList<string>>.Ok(result.Domains?.Select(d => d.Domain).ToList() ?? []);
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
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/dns/retrieve/{domain}", AuthBody, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<PbRecordsResponse>(JsonOptions, ct);
            if (result?.Status != "SUCCESS")
                return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.UnknownError, result?.Message ?? "Failed");

            var records = result.Records?.Select(r => new DnsRecordInfo(
                r.Id, domain, r.Name == domain ? "@" : r.Name.Replace($".{domain}", ""),
                r.Name, r.Type, r.Content, int.TryParse(r.Ttl, out var ttl) ? ttl : 600
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
            var body = new { apikey = Config.Id, secretapikey = Config.Secret, name = subDomain == "@" ? "" : subDomain, type = recordType, content = value, ttl = ttl.ToString() };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/dns/create/{domain}", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<PbCreateResponse>(JsonOptions, ct);
            if (result?.Status != "SUCCESS")
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, result?.Message ?? "Failed");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                result.Id?.ToString() ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
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

            var body = new { apikey = Config.Id, secretapikey = Config.Secret, name = existing.SubDomain == "@" ? "" : existing.SubDomain, type = existing.RecordType, content = value, ttl = (ttl ?? existing.Ttl).ToString() };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/dns/edit/{domain}/{recordId}", body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<PbStatusResponse>(JsonOptions, ct);
            if (result?.Status != "SUCCESS")
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, result?.Message ?? "Failed");

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
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/dns/delete/{domain}/{recordId}", AuthBody, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<PbStatusResponse>(JsonOptions, ct);
            return result?.Status == "SUCCESS" ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, result?.Message ?? "Failed");
        }
        catch (Exception ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private class PbStatusResponse { public string Status { get; set; } = ""; public string? Message { get; set; } }
    private class PbDomainsResponse : PbStatusResponse { public List<PbDomain>? Domains { get; set; } }
    private class PbDomain { public string Domain { get; set; } = ""; }
    private class PbRecordsResponse : PbStatusResponse { public List<PbRecord>? Records { get; set; } }
    private class PbRecord { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Content { get; set; } = ""; public string Ttl { get; set; } = "600"; }
    private class PbCreateResponse : PbStatusResponse { public long? Id { get; set; } }
}
