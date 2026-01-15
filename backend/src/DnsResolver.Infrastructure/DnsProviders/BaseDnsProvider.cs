namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Text.Json;
using DnsResolver.Domain.Services;

public abstract class BaseDnsProvider : IDnsProvider
{
    protected DnsProviderConfig Config { get; private set; } = null!;
    protected HttpClient HttpClient { get; }

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public abstract string Name { get; }
    public abstract string DisplayName { get; }

    protected BaseDnsProvider(HttpClient httpClient) => HttpClient = httpClient;

    public virtual void Configure(DnsProviderConfig config) =>
        Config = config ?? throw new ArgumentNullException(nameof(config));

    public abstract Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default);
    public abstract Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default);
    public abstract Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default);
    public abstract Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default);
    public abstract Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default);

    protected async Task<T?> GetJsonAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await HttpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    protected async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken ct = default)
    {
        var response = await HttpClient.PostAsJsonAsync(url, data, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
    }

    protected static string GetFullDomain(string subDomain, string domain) =>
        string.IsNullOrEmpty(subDomain) || subDomain == "@" ? domain : $"{subDomain}.{domain}";
}
