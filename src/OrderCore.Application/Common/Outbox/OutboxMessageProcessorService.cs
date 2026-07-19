using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Abstractions.Repositories;

namespace OrderCore.Application.Common.Outbox
{
    public class OutboxMessageProcessorService
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxMessagePublisher _outboxMessagePublisher;

        public OutboxMessageProcessorService(
            IOutboxRepository outboxRepository,
            IOutboxMessagePublisher outboxMessagePublisher)
        {
            _outboxRepository = outboxRepository;
            _outboxMessagePublisher = outboxMessagePublisher;
        }

        public async Task<int> ExecuteAsync(
            int batchSize,
            int maxRetryCount,
            CancellationToken cancellationToken = default)
        {
            var messages = await _outboxRepository.GetPendingAsync(
                batchSize,
                maxRetryCount,
                cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await _outboxMessagePublisher.PublishAsync(message, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await _outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        ex.Message,
                        cancellationToken);

                    continue;
                }

                await _outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
            }

            return messages.Count;
        }
    }
}
