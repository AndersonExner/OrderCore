using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Common.Outbox;

namespace OrderCore.Infrastructure.Messaging
{
    public class LoggingOutboxMessagePublisher : IOutboxMessagePublisher
    {
        private readonly ILogger<LoggingOutboxMessagePublisher> _logger;

        public LoggingOutboxMessagePublisher(ILogger<LoggingOutboxMessagePublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(
            OutboxMessageEnvelope message,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Outbox message published. Id: {MessageId}, Type: {MessageType}, Payload: {Payload}",
                message.Id,
                message.Type,
                message.Payload);

            return Task.CompletedTask;
        }
    }
}
