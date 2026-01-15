namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Text.Json;
using DnsResolver.Domain.Services;

public class CallbackProvider : BaseDnsProvider
{
    public override string Name => "callback";
    public override string DisplayName => "自定义回调";

    public CallbackProvider(HttpClient httpClient) : base(httpClient) { }

    public override Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
        => Task.FromResult(ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.InvalidParameter, "Callback provider does not support listing domains"));

    public override Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
        => Task.FromResult(ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.InvalidParameter, "Callback provider does not support listing records"));

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
        => await ExecuteCallbackAsync(domain, subDomain, recordType, value, ttl, "add", ct);

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        var parts = recordId.Split('_', 2);
        return await ExecuteCallbackAsync(domain, parts[0], parts.Length > 1 ? parts[1] : "A", value, ttl ?? 600, "update", ct);
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        var parts = recordId.Split('_', 2);
        var result = await ExecuteCallbackAsync(domain, parts[0], parts.Length > 1 ? parts[1] : "A", "", 0, "delete", ct);
        return result.Success ? ProviderResult.Ok() : ProviderResult.Fail(result.ErrorCode, result.ErrorMessage!);
    }

    private async Task<ProviderResult<DnsRecordInfo>> ExecuteCallbackAsync(string domain, string subDomain, string recordType, string value, int ttl, string action, CancellationToken ct)
    {
        try
        {
            var url = Config.ExtraParams?.GetValueOrDefault("url") ?? "";
            var method = Config.ExtraParams?.GetValueOrDefault("method") ?? "GET";
            var bodyTemplate = Config.ExtraParams?.GetValueOrDefault("body");
            var headersJson = Config.ExtraParams?.GetValueOrDefault("headers");

            if (string.IsNullOrEmpty(url))
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.InvalidParameter, "Callback URL is required");

            url = ReplaceVariables(url, domain, subDomain, recordType, value, ttl, action);
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (!string.IsNullOrEmpty(headersJson))
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson);
                if (headers != null)
                    foreach (var header in headers)
                        request.Headers.TryAddWithoutValidation(header.Key, ReplaceVariables(header.Value, domain, subDomain, recordType, value, ttl, action));
            }

            if (!string.IsNullOrEmpty(bodyTemplate) && method != "GET")
                request.Content = new StringContent(ReplaceVariables(bodyTemplate, domain, subDomain, recordType, value, ttl, action), System.Text.Encoding.UTF8, "application/json");

            var response = await HttpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo($"{subDomain}_{recordType}", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private static string ReplaceVariables(string template, string domain, string subDomain, string recordType, string value, int ttl, string action)
        => template.Replace("{domain}", domain).Replace("{subdomain}", subDomain).Replace("{type}", recordType)
            .Replace("{value}", value).Replace("{ttl}", ttl.ToString()).Replace("{action}", action)
            .Replace("{fulldomain}", string.IsNullOrEmpty(subDomain) || subDomain == "@" ? domain : $"{subDomain}.{domain}");
}
