namespace DnsResolver.Application.Commands.Auth;

using DnsResolver.Application.Services;
using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(command.Username, ct);

        if (user == null)
        {
            return new LoginResult(false, null, null, null, null, "用户名或密码错误");
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return new LoginResult(false, null, null, null, null, "用户名或密码错误");
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, ct);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new LoginResult(true, token, user.Username, user.Role, expiresAt, null);
    }
}
