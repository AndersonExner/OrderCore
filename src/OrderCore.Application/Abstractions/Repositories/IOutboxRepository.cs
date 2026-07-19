using OrderCore.Application.Common.Outbox;

namespace OrderCore.Application.Abstractions.Repositories
{
    public interface IOutboxRepository
    {
        Task AddAsync(
            string type,
            string payload,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OutboxMessageEnvelope>> GetPendingAsync(
            int batchSize,
            int maxRetryCount,
            CancellationToken cancellationToken = default);

        Task MarkAsProcessedAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task MarkAsFailedAsync(
            Guid id,
            string error,
            CancellationToken cancellationToken = default);
    }
}
