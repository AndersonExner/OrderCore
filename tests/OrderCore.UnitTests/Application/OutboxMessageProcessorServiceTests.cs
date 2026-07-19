using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Outbox;

namespace OrderCore.UnitTests.Application
{
    public class OutboxMessageProcessorServiceTests
    {
        private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
        private readonly Mock<IOutboxMessagePublisher> _outboxMessagePublisherMock;
        private readonly OutboxMessageProcessorService _service;

        public OutboxMessageProcessorServiceTests()
        {
            _outboxRepositoryMock = new Mock<IOutboxRepository>();
            _outboxMessagePublisherMock = new Mock<IOutboxMessagePublisher>();

            _service = new OutboxMessageProcessorService(
                _outboxRepositoryMock.Object,
                _outboxMessagePublisherMock.Object,
                NullLogger<OutboxMessageProcessorService>.Instance);
        }

        [Fact]
        public async Task Should_Publish_And_Mark_Message_As_Processed()
        {
            // Arrange
            var message = new OutboxMessageEnvelope(
                Guid.NewGuid(),
                OutboxMessageTypes.OrderPaid,
                "{}",
                0,
                DateTime.UtcNow);

            _outboxRepositoryMock
                .Setup(x => x.GetPendingAsync(20, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OutboxMessageEnvelope> { message });

            // Act
            var processedCount = await _service.ExecuteAsync(20, 5, CancellationToken.None);

            // Assert
            processedCount.Should().Be(1);

            _outboxMessagePublisherMock.Verify(
                x => x.PublishAsync(message, It.IsAny<CancellationToken>()),
                Times.Once);

            _outboxRepositoryMock.Verify(
                x => x.MarkAsProcessedAsync(message.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Mark_Message_As_Failed_When_Publish_Fails()
        {
            // Arrange
            var message = new OutboxMessageEnvelope(
                Guid.NewGuid(),
                OutboxMessageTypes.OrderPaid,
                "{}",
                0,
                DateTime.UtcNow);

            _outboxRepositoryMock
                .Setup(x => x.GetPendingAsync(20, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OutboxMessageEnvelope> { message });

            _outboxMessagePublisherMock
                .Setup(x => x.PublishAsync(message, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Publisher unavailable."));

            // Act
            var processedCount = await _service.ExecuteAsync(20, 5, CancellationToken.None);

            // Assert
            processedCount.Should().Be(1);

            _outboxRepositoryMock.Verify(
                x => x.MarkAsFailedAsync(
                    message.Id,
                    "Publisher unavailable.",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _outboxRepositoryMock.Verify(
                x => x.MarkAsProcessedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Not_Mark_Message_As_Failed_When_Marking_As_Processed_Fails()
        {
            // Arrange
            var message = new OutboxMessageEnvelope(
                Guid.NewGuid(),
                OutboxMessageTypes.OrderPaid,
                "{}",
                0,
                DateTime.UtcNow);

            _outboxRepositoryMock
                .Setup(x => x.GetPendingAsync(20, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OutboxMessageEnvelope> { message });

            _outboxRepositoryMock
                .Setup(x => x.MarkAsProcessedAsync(message.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database unavailable."));

            // Act
            Func<Task> action = async () => await _service.ExecuteAsync(20, 5, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Database unavailable*");

            _outboxRepositoryMock.Verify(
                x => x.MarkAsFailedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
