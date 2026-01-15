namespace DnsResolver.Application.Commands.Auth;

using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;

public class ChangePasswordCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResult> HandleAsync(ChangePasswordCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);

        if (user == null)
        {
            return new ChangePasswordResult(false, "用户不存在");
        }

        if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return new ChangePasswordResult(false, "当前密码错误");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 6)
        {
            return new ChangePasswordResult(false, "新密码长度至少为6位");
        }

        var newHash = _passwordHasher.Hash(command.NewPassword);
        user.UpdatePassword(newHash);
        await _userRepository.UpdateAsync(user, ct);

        return new ChangePasswordResult(true, null);
    }
}
