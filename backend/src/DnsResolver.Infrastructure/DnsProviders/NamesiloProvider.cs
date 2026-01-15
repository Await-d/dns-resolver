namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using DnsResolver.Domain.Services;

public class NamesiloProvider : BaseDnsProvider
{
    private const string Endpoint = "https://www.namesilo.com/api";

    public override string Name => "namesilo";
    public override string DisplayName => "NameSilo";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: null,
        SecretLabel: "API Key",
        HelpUrl: "https://www.namesilo.com/account/api-manager",
        HelpText: "NameSilo 的 TTL 最低 1 小时"
    );

    public NamesiloProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var xml = await HttpClient.GetStringAsync($"{Endpoint}/listDomains?version=1&type=xml&key={Config.Secret}", ct);
            var doc = XDocument.Parse(xml);
            var domains = doc.Descendants("domain").Select(d => d.Value).ToList();
            return ProviderResult<IReadOnlyList<string>>.Ok(domains);
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
            var xml = await HttpClient.GetStringAsync($"{Endpoint}/dnsListRecords?version=1&type=xml&key={Config.Secret}&domain={domain}", ct);
            var doc = XDocument.Parse(xml);
            var records = doc.Descendants("resource_record").Select(r => new DnsRecordInfo(
                r.Element("record_id")?.Value ?? "",
                domain,
                r.Element("host")?.Value?.Replace($".{domain}", "") ?? "",
                r.Element("host")?.Value ?? "",
                r.Element("type")?.Value ?? "",
                r.Element("value")?.Value ?? "",
                int.TryParse(r.Element("ttl")?.Value, out var ttl) ? ttl : 7200
            )).ToList();

            if (!string.IsNullOrEmpty(subDomain))
                records = records.Where(r => r.SubDomain == subDomain).ToList();
            if (!string.IsNullOrEmpty(recordType))
                records = records.Where(r => r.RecordType == recordType).ToList();

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
            var host = subDomain == "@" ? "" : subDomain;
            var url = $"{Endpoint}/dnsAddRecord?version=1&type=xml&key={Config.Secret}&domain={domain}&rrtype={recordType}&rrhost={host}&rrvalue={Uri.EscapeDataString(value)}&rrttl={ttl}";
            var xml = await HttpClient.GetStringAsync(url, ct);
            var doc = XDocument.Parse(xml);
            var recordId = doc.Descendants("record_id").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(recordId))
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed to add record");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                recordId, domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
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

            var host = existing.SubDomain == "@" ? "" : existing.SubDomain;
            var url = $"{Endpoint}/dnsUpdateRecord?version=1&type=xml&key={Config.Secret}&domain={domain}&rrid={recordId}&rrhost={host}&rrvalue={Uri.EscapeDataString(value)}&rrttl={ttl ?? existing.Ttl}";
            await HttpClient.GetStringAsync(url, ct);

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
            await HttpClient.GetStringAsync($"{Endpoint}/dnsDeleteRecord?version=1&type=xml&key={Config.Secret}&domain={domain}&rrid={recordId}", ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }
}
