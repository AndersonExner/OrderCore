using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Logging;

namespace OrderCore.Application.Common.Outbox
{
    public class OutboxMessageProcessorService
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly IOutboxMessagePublisher _outboxMessagePublisher;
        private readonly ILogger<OutboxMessageProcessorService> _logger;

        public OutboxMessageProcessorService(
            IOutboxRepository outboxRepository,
            IOutboxMessagePublisher outboxMessagePublisher,
            ILogger<OutboxMessageProcessorService> logger)
        {
            _outboxRepository = outboxRepository;
            _outboxMessagePublisher = outboxMessagePublisher;
            _logger = logger;
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
                    _logger.LogError(
                        ApplicationLogEvents.OutboxMessagePublishFailed,
                        ex,
                        "Outbox message publishing failed. OutboxMessageId: {OutboxMessageId}, Type: {OutboxMessageType}, RetryCount: {RetryCount}",
                        message.Id,
                        message.Type,
                        message.RetryCount);

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
