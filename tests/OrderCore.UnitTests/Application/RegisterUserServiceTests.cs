using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Abstractions.Security;
using OrderCore.Application.Auth;
using OrderCore.Application.Auth.Commands;
using OrderCore.Application.Auth.Dtos;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application;

public class RegisterUserServiceTests
{
    private readonly Mock<IApplicationUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly RegisterUserService _service;

    public RegisterUserServiceTests()
    {
        _passwordHasherMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("hashed-password");

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(It.IsAny<ApplicationUser>()))
            .Returns(("token", DateTime.UtcNow.AddHours(1)));

        _service = new RegisterUserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            NullLogger<RegisterUserService>.Instance);
    }

    [Fact]
    public async Task Should_Register_User_When_Data_Is_Valid()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            UserName = "admin",
            Email = "ADMIN@EMAIL.COM",
            Password = "Admin123!",
            Role = AuthRoles.Admin
        };

        // Act
        var response = await _service.ExecuteAsync(request);

        // Assert
        response.AccessToken.Should().Be("token");
        response.User.UserName.Should().Be("admin");
        response.User.Email.Should().Be("admin@email.com");
        response.User.Role.Should().Be(AuthRoles.Admin);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Throw_When_Role_Is_Invalid()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            UserName = "operator",
            Email = "operator@email.com",
            Password = "Admin123!",
            Role = "InvalidRole"
        };

        // Act
        var action = async () => await _service.ExecuteAsync(request);

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid user role*");
    }

    [Fact]
    public async Task Should_Throw_When_UserName_Already_Exists()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            UserName = "admin",
            Email = "admin@email.com",
            Password = "Admin123!",
            Role = AuthRoles.Admin
        };

        _userRepositoryMock
            .Setup(x => x.GetByUserNameAsync(request.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser("admin", "other@email.com", "hash", AuthRoles.Admin));

        // Act
        var action = async () => await _service.ExecuteAsync(request);

        // Assert
        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*user name already exists*");
    }
}
