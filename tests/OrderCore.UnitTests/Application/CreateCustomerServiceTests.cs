using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Dtos;
using OrderCore.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderCore.UnitTests.Application
{
    public class CreateCustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly CreateCustomerService _service;

        public CreateCustomerServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _service = new CreateCustomerService(_customerRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Create_Customer_When_Email_Does_Not_Exist()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                Name = "test",
                Email = "test@email.com"
            };

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Name.Should().Be(request.Name);
            response.Email.Should().Be(request.Email.ToLowerInvariant());
            response.Id.Should().NotBeEmpty();

            _customerRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_When_Email_Already_Exists()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                Name = "test",
                Email = "test@email.com"
            };

            var existingCustomer = new Customer("Existing User", "test@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");

            _customerRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}