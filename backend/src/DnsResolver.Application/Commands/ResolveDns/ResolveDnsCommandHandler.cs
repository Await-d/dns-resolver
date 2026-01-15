namespace DnsResolver.Application.Commands.ResolveDns;

using DnsResolver.Application.DTOs;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Services;
using DnsResolver.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

public class ResolveDnsCommandHandler
{
    private readonly IDnsResolutionService _resolutionService;
    private readonly IIspProviderRepository _ispRepository;
    private readonly ILogger<ResolveDnsCommandHandler> _logger;

    public ResolveDnsCommandHandler(
        IDnsResolutionService resolutionService,
        IIspProviderRepository ispRepository,
        ILogger<ResolveDnsCommandHandler> logger)
    {
        _resolutionService = resolutionService;
        _ispRepository = ispRepository;
        _logger = logger;
    }

    public async Task<ResolveDnsResult> HandleAsync(
        ResolveDnsCommand command,
        CancellationToken cancellationToken)
    {
        var domain = DomainName.Create(command.Domain);
        var recordType = RecordType.Create(command.RecordType);
        var dnsServer = DnsServer.Create(command.DnsServer);

        _logger.LogInformation("解析 {Domain} ({RecordType}) via {DnsServer}",
            domain, recordType, dnsServer);

        var isp = await _ispRepository.FindByDnsServerAsync(command.DnsServer, cancellationToken);
        var ispName = isp?.Name;

        var query = await _resolutionService.ResolveAsync(
            domain, recordType, dnsServer, ispName, cancellationToken);

        var records = query.Result?.Records
            .Select(r => new DnsRecordDto(r.Value, r.Ttl, r.Type.Value))
            .ToList() ?? [];

        return new ResolveDnsResult(
            Domain: query.Domain.Value,
            RecordType: query.RecordType.Value,
            DnsServer: query.DnsServer.Address,
            IspName: query.IspName ?? "自定义DNS",
            Records: records,
            QueryTimeMs: query.Result?.QueryTimeMs ?? 0,
            Success: query.Result?.Success ?? false,
            ErrorMessage: query.Result?.ErrorMessage
        );
    }
}
