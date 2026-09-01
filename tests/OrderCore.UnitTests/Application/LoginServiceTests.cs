using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Abstractions.Security;
using OrderCore.Application.Auth;
using OrderCore.Application.Auth.Commands;
using OrderCore.Application.Auth.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application;

public class LoginServiceTests
{
    private readonly Mock<IApplicationUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly LoginService _service;

    public LoginServiceTests()
    {
        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(It.IsAny<ApplicationUser>()))
            .Returns(("token", DateTime.UtcNow.AddHours(1)));

        _service = new LoginService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            NullLogger<LoginService>.Instance);
    }

    [Fact]
    public async Task Should_Return_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var user = new ApplicationUser("admin", "admin@email.com", "hash", AuthRoles.Admin);
        var request = new LoginRequest
        {
            UserNameOrEmail = "admin",
            Password = "Admin123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUserNameOrEmailAsync(request.UserNameOrEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var response = await _service.ExecuteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response!.AccessToken.Should().Be("token");
        response.User.Role.Should().Be(AuthRoles.Admin);
    }

    [Fact]
    public async Task Should_Return_Null_When_Credentials_Are_Invalid()
    {
        // Arrange
        var user = new ApplicationUser("admin", "admin@email.com", "hash", AuthRoles.Admin);
        var request = new LoginRequest
        {
            UserNameOrEmail = "admin",
            Password = "wrong-password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUserNameOrEmailAsync(request.UserNameOrEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var response = await _service.ExecuteAsync(request);

        // Assert
        response.Should().BeNull();
    }
}
