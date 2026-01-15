namespace DnsResolver.Domain.Aggregates.User;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public User(Guid id, string username, string passwordHash, string role)
    {
        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role ?? throw new ArgumentNullException(nameof(role));
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string username, string passwordHash, string role = "user")
    {
        return new User(Guid.NewGuid(), username, passwordHash, role);
    }

    public static User CreateAdmin(string username, string passwordHash)
    {
        return new User(Guid.NewGuid(), username, passwordHash, "admin");
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
