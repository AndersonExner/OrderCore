using FluentAssertions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Orders.Commands;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application
{
    public class PayOrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly PayOrderService _service;

        public PayOrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _service = new PayOrderService(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task Should_Mark_Order_As_Paid_When_Order_Is_Pending()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            var response = await _service.ExecuteAsync(order.Id, CancellationToken.None);

            // Assert
            response.Status.Should().Be("Paid");

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

            _orderRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Throw_When_Order_Is_Cancelled()
        {
            // Arrange
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), "Notebook Dell", 4500m, 1);
            order.Cancel();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(order.Id, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("*pending*");

            _orderRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
