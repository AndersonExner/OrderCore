using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Abstractions.Security;
using OrderCore.Application.Auth.Dtos;

namespace OrderCore.Application.Auth.Commands;

public class LoginService
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginService> _logger;

    public LoginService(
        IApplicationUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse?> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUserNameOrEmailAsync(
            request.UserNameOrEmail,
            cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning(
                "Login rejected. UserNameOrEmail: {UserNameOrEmail}",
                request.UserNameOrEmail);

            return null;
        }

        _logger.LogInformation(
            "User authenticated. UserId: {UserId}, UserName: {UserName}, Role: {Role}",
            user.Id,
            user.UserName,
            user.Role);

        var token = _jwtTokenGenerator.Generate(user);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            }
        };
    }
}
