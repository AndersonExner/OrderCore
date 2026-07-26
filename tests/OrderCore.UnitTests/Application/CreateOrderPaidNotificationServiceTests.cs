using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Notifications.Commands;
using OrderCore.Contracts.Events;
using OrderCore.Domain.Entities;

namespace OrderCore.UnitTests.Application
{
    public class CreateOrderPaidNotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly CreateOrderPaidNotificationService _service;

        public CreateOrderPaidNotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _service = new CreateOrderPaidNotificationService(
                _notificationRepositoryMock.Object,
                NullLogger<CreateOrderPaidNotificationService>.Instance);
        }

        [Fact]
        public async Task Should_Create_Notification_When_Source_Message_Was_Not_Processed()
        {
            // Arrange
            var sourceMessageId = Guid.NewGuid();
            var integrationEvent = new OrderPaidIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                5000m,
                DateTime.UtcNow);

            _notificationRepositoryMock
                .Setup(x => x.ExistsBySourceMessageIdAsync(sourceMessageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var created = await _service.ExecuteAsync(
                integrationEvent,
                sourceMessageId,
                CancellationToken.None);

            // Assert
            created.Should().BeTrue();

            _notificationRepositoryMock.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(notification =>
                        notification.SourceMessageId == sourceMessageId &&
                        notification.OrderId == integrationEvent.OrderId &&
                        notification.CustomerId == integrationEvent.CustomerId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Not_Create_Notification_When_Source_Message_Was_Already_Processed()
        {
            // Arrange
            var sourceMessageId = Guid.NewGuid();
            var integrationEvent = new OrderPaidIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                5000m,
                DateTime.UtcNow);

            _notificationRepositoryMock
                .Setup(x => x.ExistsBySourceMessageIdAsync(sourceMessageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var created = await _service.ExecuteAsync(
                integrationEvent,
                sourceMessageId,
                CancellationToken.None);

            // Assert
            created.Should().BeFalse();

            _notificationRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
