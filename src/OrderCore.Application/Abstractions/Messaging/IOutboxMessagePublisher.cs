using OrderCore.Application.Common.Outbox;

namespace OrderCore.Application.Abstractions.Messaging
{
    public interface IOutboxMessagePublisher
    {
        Task PublishAsync(
            OutboxMessageEnvelope message,
            CancellationToken cancellationToken = default);
    }
}
