using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Orders.Commands;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application
{
    public class CancelOrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly CancelOrderService _service;

        public CancelOrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();

            _service = new CancelOrderService(
                _orderRepositoryMock.Object,
                _productRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Cancel_Pending_Order_And_Restore_Stock()
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 3);
            var order = new Order(Guid.NewGuid());
            order.AddItem(product.Id, product.Name, product.Price, 2);

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _productRepositoryMock
                .Setup(x => x.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(product.Id)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product> { product });

            // Act
            var response = await _service.ExecuteAsync(order.Id, CancellationToken.None);

            // Assert
            response.Status.Should().Be("Cancelled");
            product.StockQuantity.Should().Be(5);

            _orderRepositoryMock.Verify(
                x => x.UpdateAsync(order, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_When_Order_Does_Not_Exist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(orderId, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Order not found*");
        }

        [Fact]
        public async Task Should_Throw_When_Order_Is_Paid()
        {
            // Arrange
            var product = new Product("Notebook Dell", 4500m, 3);
            var order = new Order(Guid.NewGuid());
            order.AddItem(product.Id, product.Name, product.Price, 2);
            order.MarkAsPaid();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(order.Id, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("*cannot be cancelled*");

            _productRepositoryMock.Verify(
                x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
