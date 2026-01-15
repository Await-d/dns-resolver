namespace DnsResolver.Application.Commands.CompareDns;

using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Application.DTOs;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Services;
using DnsResolver.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

public class CompareDnsCommandHandler
{
    private readonly IDnsResolutionService _resolutionService;
    private readonly IIspProviderRepository _ispRepository;
    private readonly ILogger<CompareDnsCommandHandler> _logger;

    public CompareDnsCommandHandler(
        IDnsResolutionService resolutionService,
        IIspProviderRepository ispRepository,
        ILogger<CompareDnsCommandHandler> logger)
    {
        _resolutionService = resolutionService;
        _ispRepository = ispRepository;
        _logger = logger;
    }

    public async Task<CompareDnsResult> HandleAsync(
        CompareDnsCommand command,
        CancellationToken cancellationToken)
    {
        var domain = DomainName.Create(command.Domain);
        var recordType = RecordType.Create(command.RecordType);

        var providers = new List<IspProvider>();
        foreach (var ispId in command.IspIds)
        {
            var provider = await _ispRepository.FindByIdAsync(ispId, cancellationToken);
            if (provider != null)
                providers.Add(provider);
        }

        _logger.LogInformation("批量解析 {Domain} ({RecordType}) via {Count} ISPs",
            domain, recordType, providers.Count);

        var queries = await _resolutionService.BatchResolveAsync(
            domain, recordType, providers, cancellationToken);

        var results = queries.Select(q => new ResolveDnsResult(
            Domain: q.Domain.Value,
            RecordType: q.RecordType.Value,
            DnsServer: q.DnsServer.Address,
            IspName: q.IspName ?? "自定义DNS",
            Records: q.Result?.Records
                .Select(r => new DnsRecordDto(r.Value, r.Ttl, r.Type.Value))
                .ToList() ?? [],
            QueryTimeMs: q.Result?.QueryTimeMs ?? 0,
            Success: q.Result?.Success ?? false,
            ErrorMessage: q.Result?.ErrorMessage
        )).ToList();

        return new CompareDnsResult(domain.Value, recordType.Value, results);
    }
}
