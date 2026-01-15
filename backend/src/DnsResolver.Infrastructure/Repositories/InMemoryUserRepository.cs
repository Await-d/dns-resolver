namespace DnsResolver.Infrastructure.Repositories;

using System.Collections.Concurrent;
using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly IPasswordHasher _passwordHasher;

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        InitializeDefaultAdmin();
    }

    private void InitializeDefaultAdmin()
    {
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var passwordHash = _passwordHasher.Hash("admin123");
        var admin = new User(adminId, "admin", passwordHash, "admin");
        _users.TryAdd(adminId, admin);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<User> users = _users.Values.ToList();
        return Task.FromResult(users);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users.TryAdd(user.Id, user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _users.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        var exists = _users.Values.Any(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}
