using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Abstractions.Security;
using OrderCore.Application.Auth.Dtos;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Auth.Commands;

public class RegisterUserService
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RegisterUserService> _logger;

    public RegisterUserService(
        IApplicationUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RegisterUserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Password must have at least 8 characters.");

        if (!AuthRoles.IsValid(request.Role))
            throw new ValidationException("Invalid user role.");

        var existingByUserName = await _userRepository.GetByUserNameAsync(
            request.UserName,
            cancellationToken);

        if (existingByUserName is not null)
            throw new BusinessRuleException("A user with the same user name already exists.");

        var existingByEmail = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (existingByEmail is not null)
            throw new BusinessRuleException("A user with the same email already exists.");

        var normalizedRole = AuthRoles.Normalize(request.Role);
        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new ApplicationUser(request.UserName, request.Email, passwordHash, normalizedRole);

        await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation(
            "User registered. UserId: {UserId}, UserName: {UserName}, Role: {Role}",
            user.Id,
            user.UserName,
            user.Role);

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user)
    {
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
