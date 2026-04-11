using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Customers.Queries;
using OrderCore.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderCore.UnitTests.Application
{
    public class GetCustomerByIdServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly GetCustomerByIdService _service;

        public GetCustomerByIdServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _service = new GetCustomerByIdService(_customerRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Return_Customer_When_Found()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer("test", "test@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var response = await _service.ExecuteAsync(customerId, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(customer.Id);
            response.Name.Should().Be(customer.Name);
            response.Email.Should().Be(customer.Email);
        }

        [Fact]
        public async Task Should_Return_Null_When_Customer_Is_Not_Found()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            // Act
            var response = await _service.ExecuteAsync(customerId, CancellationToken.None);

            // Assert
            response.Should().BeNull();
        }
    }
}