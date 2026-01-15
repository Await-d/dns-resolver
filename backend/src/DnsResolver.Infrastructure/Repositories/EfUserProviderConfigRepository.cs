namespace DnsResolver.Infrastructure.Repositories;

using DnsResolver.Domain.Aggregates.UserProviderConfig;
using DnsResolver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EfUserProviderConfigRepository : IUserProviderConfigRepository
{
    private readonly AppDbContext _context;

    public EfUserProviderConfigRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserProviderConfig>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProviderConfigs
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserProviderConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserProviderConfigs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<UserProviderConfig?> GetByUserAndProviderAsync(Guid userId, string providerName, CancellationToken cancellationToken = default)
    {
        return await _context.UserProviderConfigs
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProviderName == providerName, cancellationToken);
    }

    public async Task AddAsync(UserProviderConfig config, CancellationToken cancellationToken = default)
    {
        await _context.UserProviderConfigs.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserProviderConfig config, CancellationToken cancellationToken = default)
    {
        _context.UserProviderConfigs.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _context.UserProviderConfigs.FindAsync(new object[] { id }, cancellationToken);
        if (config != null)
        {
            _context.UserProviderConfigs.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
