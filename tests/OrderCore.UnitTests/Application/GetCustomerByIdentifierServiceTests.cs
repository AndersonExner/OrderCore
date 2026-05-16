using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Customers.Queries;
using OrderCore.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderCore.UnitTests.Application
{
    public class GetCustomerByIdentifierServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly GetCustomerByIdentifierService _service;

        public GetCustomerByIdentifierServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _service = new GetCustomerByIdentifierService(_customerRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Return_Customer_When_Search_Term_Is_Id()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer("test", "test@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var response = await _service.ExecuteAsync(customerId.ToString(), CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(customer.Id);
            response.Name.Should().Be(customer.Name);
            response.Email.Should().Be(customer.Email);

            _customerRepositoryMock.Verify(
                x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _customerRepositoryMock.Verify(
                x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Return_Customer_When_Search_Term_Is_Email()
        {
            // Arrange
            var customer = new Customer("test", "test@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var response = await _service.ExecuteAsync(" TEST@EMAIL.COM ", CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(customer.Id);
            response.Name.Should().Be(customer.Name);
            response.Email.Should().Be(customer.Email);

            _customerRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _customerRepositoryMock.Verify(
                x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Return_Customer_When_Search_Term_Is_Partial_Name()
        {
            // Arrange
            var customer = new Customer("Anderson", "anderson@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByNameAsync("ander", It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var response = await _service.ExecuteAsync(" ander ", CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(customer.Id);
            response.Name.Should().Be(customer.Name);
            response.Email.Should().Be(customer.Email);

            _customerRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _customerRepositoryMock.Verify(
                x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
            var response = await _service.ExecuteAsync(customerId.ToString(), CancellationToken.None);

            // Assert
            response.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Should_Throw_When_Search_Term_Is_Invalid(string term)
        {
            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(term, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ValidationException>()
                .WithMessage("*valid customer id, email, or name*");

            _customerRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _customerRepositoryMock.Verify(
                x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _customerRepositoryMock.Verify(
                x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
