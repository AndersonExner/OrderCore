using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Common.Outbox;
using RabbitMQ.Client;

namespace OrderCore.Infrastructure.Messaging
{
    public class RabbitMqOutboxMessagePublisher : IOutboxMessagePublisher
    {
        private readonly RabbitMqConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqOutboxMessagePublisher> _logger;

        public RabbitMqOutboxMessagePublisher(
            RabbitMqConnection connection,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqOutboxMessagePublisher> logger)
        {
            _connection = connection;
            _options = options.Value;
            _logger = logger;
        }

        public Task PublishAsync(
            OutboxMessageEnvelope message,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var channel = _connection.GetOpenConnection().CreateModel();

            if (_options.DeclareTopology)
            {
                DeclareTopology(channel);
            }

            var routingKey = GetRoutingKey(message.Type);
            var body = Encoding.UTF8.GetBytes(message.Payload);
            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = message.Id.ToString();
            properties.Type = message.Type;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["outbox-message-id"] = message.Id.ToString(),
                ["outbox-message-type"] = message.Type,
                ["outbox-retry-count"] = message.RetryCount
            };

            channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Outbox message published to RabbitMQ. OutboxMessageId: {OutboxMessageId}, Type: {OutboxMessageType}, Exchange: {RabbitMqExchange}, RoutingKey: {RabbitMqRoutingKey}",
                message.Id,
                message.Type,
                _options.ExchangeName,
                routingKey);

            return Task.CompletedTask;
        }

        private void DeclareTopology(IModel channel)
        {
            channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: true,
                autoDelete: false);

            if (string.IsNullOrWhiteSpace(_options.OrderPaidQueueName))
            {
                return;
            }

            channel.QueueDeclare(
                queue: _options.OrderPaidQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            channel.QueueBind(
                queue: _options.OrderPaidQueueName,
                exchange: _options.ExchangeName,
                routingKey: GetRoutingKey(OutboxMessageTypes.OrderPaid));
        }

        private static string GetRoutingKey(string messageType)
        {
            return messageType switch
            {
                OutboxMessageTypes.OrderCreated => "orders.created",
                OutboxMessageTypes.OrderPaid => "orders.paid",
                OutboxMessageTypes.OrderCancelled => "orders.cancelled",
                _ => $"events.{messageType.ToLowerInvariant()}"
            };
        }
    }
}
