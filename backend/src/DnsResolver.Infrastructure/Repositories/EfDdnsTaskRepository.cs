namespace DnsResolver.Infrastructure.Repositories;

using DnsResolver.Domain.Aggregates.DdnsTask;
using DnsResolver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EfDdnsTaskRepository : IDdnsTaskRepository
{
    private readonly AppDbContext _context;

    public EfDdnsTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DdnsTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DdnsTasks.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<DdnsTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DdnsTasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DdnsTask>> GetEnabledTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DdnsTasks
            .Where(t => t.Enabled)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DdnsTask> AddAsync(DdnsTask task, CancellationToken cancellationToken = default)
    {
        await _context.DdnsTasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateAsync(DdnsTask task, CancellationToken cancellationToken = default)
    {
        _context.DdnsTasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _context.DdnsTasks.FindAsync([id], cancellationToken);
        if (task != null)
        {
            _context.DdnsTasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
