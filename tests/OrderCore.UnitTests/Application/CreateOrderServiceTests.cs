using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Commom.Exceptions;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application
{
    public class CreateOrderServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly CreateOrderService _service;

        public CreateOrderServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();

            _service = new CreateOrderService(
                _customerRepositoryMock.Object,
                _productRepositoryMock.Object,
                _orderRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Throw_When_Order_Has_No_Items()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid(),
                Items = new System.Collections.Generic.List<CreateOrderItemRequest>()
            };

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ValidationException>()
                .WithMessage("*at least one item*");
        }

        [Fact]
        public async Task Should_Throw_When_Customer_Does_Not_Exist()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid(),
                Items = new System.Collections.Generic.List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 2
                    }
                }
            };

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Customer not found*");
        }

        [Fact]
        public async Task Should_Throw_When_Product_Does_Not_Exist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                Items = new System.Collections.Generic.List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ProductId = productId,
                        Quantity = 1
                    }
                }
            };

            var customer = new Customer("test", "test@email.com");

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*Product {productId} not found*");
        }

        [Fact]
        public async Task Should_Create_Order_Successfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                Items = new System.Collections.Generic.List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ProductId = productId,
                        Quantity = 2
                    }
                }
            };

            var customer = new Customer("Test", "test@email.com");
            var product = new Product("Notebook Dell", 4500m, 10);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.CustomerId.Should().Be(customerId);
            response.Status.Should().Be("Pending");
            response.Items.Should().HaveCount(1);
            response.TotalAmount.Should().Be(9000m);

            var item = response.Items.First();
            item.ProductId.Should().Be(product.Id);
            item.ProductName.Should().Be(product.Name);
            item.UnitPrice.Should().Be(product.Price);
            item.Quantity.Should().Be(2);
            item.Total.Should().Be(9000m);

            _orderRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Calculate_TotalAmount_Correctly_With_Multiple_Items()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();

            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                Items = new System.Collections.Generic.List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ProductId = productId1,
                        Quantity = 1
                    },
                    new CreateOrderItemRequest
                    {
                        ProductId = productId2,
                        Quantity = 2
                    }
                }
            };

            var customer = new Customer("test", "test@email.com");
            var product1 = new Product("Notebook Dell", 4500m, 10);
            var product2 = new Product("Mouse Logitech", 150m, 10);

            _customerRepositoryMock
                .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(productId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product1);

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(productId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product2);

            _orderRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _service.ExecuteAsync(request, CancellationToken.None);

            // Assert
            response.TotalAmount.Should().Be(4800m);
            response.Items.Should().HaveCount(2);
        }
    }
}