namespace DnsResolver.Infrastructure.DnsProviders;

using DnsResolver.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public class DnsProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _providerTypes = new(StringComparer.OrdinalIgnoreCase);

    public DnsProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterAllProviders();
    }

    private void RegisterAllProviders()
    {
        RegisterProvider<AlidnsProvider>("alidns");
        RegisterProvider<AliesaProvider>("aliesa");
        RegisterProvider<BaiduCloudProvider>("baiducloud");
        RegisterProvider<CallbackProvider>("callback");
        RegisterProvider<CloudflareProvider>("cloudflare");
        RegisterProvider<DnslaProvider>("dnsla");
        RegisterProvider<DnspodProvider>("dnspod");
        RegisterProvider<DynadotProvider>("dynadot");
        RegisterProvider<Dynv6Provider>("dynv6");
        RegisterProvider<EdgeOneProvider>("edgeone");
        RegisterProvider<EranetProvider>("eranet");
        RegisterProvider<GcoreProvider>("gcore");
        RegisterProvider<GodaddyProvider>("godaddy");
        RegisterProvider<HuaweicloudProvider>("huaweicloud");
        RegisterProvider<NamecheapProvider>("namecheap");
        RegisterProvider<NamesiloProvider>("namesilo");
        RegisterProvider<NowcnProvider>("nowcn");
        RegisterProvider<NSOneProvider>("nsone");
        RegisterProvider<PorkbunProvider>("porkbun");
        RegisterProvider<SpaceshipProvider>("spaceship");
        RegisterProvider<TencentCloudProvider>("tencentcloud");
        RegisterProvider<TrafficRouteProvider>("trafficroute");
        RegisterProvider<VercelProvider>("vercel");
    }

    public void RegisterProvider<T>(string name) where T : IDnsProvider
    {
        _providerTypes[name] = typeof(T);
    }

    public IDnsProvider? CreateProvider(string name, DnsProviderConfig config)
    {
        if (!_providerTypes.TryGetValue(name, out var providerType))
            return null;

        var provider = _serviceProvider.GetService(providerType) as IDnsProvider;
        provider?.Configure(config);
        return provider;
    }

    public IEnumerable<string> GetRegisteredProviders() => _providerTypes.Keys;

    public IEnumerable<(string Name, string DisplayName)> GetProviderInfos()
    {
        foreach (var (name, type) in _providerTypes)
        {
            var provider = _serviceProvider.GetService(type) as IDnsProvider;
            if (provider != null)
                yield return (name, provider.DisplayName);
        }
    }
}

public static class DnsProviderServiceExtensions
{
    public static IServiceCollection AddDnsProviders(this IServiceCollection services)
    {
        services.AddHttpClient<AlidnsProvider>();
        services.AddHttpClient<AliesaProvider>();
        services.AddHttpClient<BaiduCloudProvider>();
        services.AddHttpClient<CallbackProvider>();
        services.AddHttpClient<CloudflareProvider>();
        services.AddHttpClient<DnslaProvider>();
        services.AddHttpClient<DnspodProvider>();
        services.AddHttpClient<DynadotProvider>();
        services.AddHttpClient<Dynv6Provider>();
        services.AddHttpClient<EdgeOneProvider>();
        services.AddHttpClient<EranetProvider>();
        services.AddHttpClient<GcoreProvider>();
        services.AddHttpClient<GodaddyProvider>();
        services.AddHttpClient<HuaweicloudProvider>();
        services.AddHttpClient<NamecheapProvider>();
        services.AddHttpClient<NamesiloProvider>();
        services.AddHttpClient<NowcnProvider>();
        services.AddHttpClient<NSOneProvider>();
        services.AddHttpClient<PorkbunProvider>();
        services.AddHttpClient<SpaceshipProvider>();
        services.AddHttpClient<TencentCloudProvider>();
        services.AddHttpClient<TrafficRouteProvider>();
        services.AddHttpClient<VercelProvider>();

        services.AddSingleton<DnsProviderFactory>();

        return services;
    }
}
