namespace DnsResolver.Application.Queries.GetIsps;

using DnsResolver.Application.DTOs;
using DnsResolver.Domain.Aggregates.IspProvider;

public class GetIspsQueryHandler
{
    private readonly IIspProviderRepository _repository;

    public GetIspsQueryHandler(IIspProviderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<IspProviderDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _repository.GetAllAsync(cancellationToken);

        return providers.Select(p => new IspProviderDto(
            p.Id,
            p.Name,
            p.PrimaryDns.Address,
            p.SecondaryDns?.Address
        )).ToList();
    }
}
