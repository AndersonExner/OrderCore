using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderCore.Application.Notifications.Commands;
using OrderCore.Contracts.Events;
using OrderCore.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderCore.Worker
{
    public class OrderPaidNotificationConsumerService : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly RabbitMqConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OrderPaidNotificationConsumerService> _logger;

        public OrderPaidNotificationConsumerService(
            RabbitMqConnection connection,
            IOptions<RabbitMqOptions> options,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OrderPaidNotificationConsumerService> logger)
        {
            _connection = connection;
            _options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var channel = _connection.GetOpenConnection().CreateModel();

                    DeclareTopology(channel);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += (_, eventArgs) => HandleMessageAsync(channel, eventArgs, stoppingToken);

                    var consumerTag = channel.BasicConsume(
                        queue: _options.OrderPaidQueueName,
                        autoAck: false,
                        consumer: consumer);

                    _logger.LogInformation(
                        "Order paid notification consumer started. Queue: {RabbitMqQueue}, ConsumerTag: {ConsumerTag}",
                        _options.OrderPaidQueueName,
                        consumerTag);

                    await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Order paid notification consumer failed. Queue: {RabbitMqQueue}",
                        _options.OrderPaidQueueName);

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task HandleMessageAsync(
            IModel channel,
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken)
        {
            try
            {
                var sourceMessageId = GetSourceMessageId(eventArgs);
                var integrationEvent = DeserializeEvent(eventArgs);

                Validate(integrationEvent);

                _logger.LogInformation(
                    "Processing order paid event. SourceMessageId: {SourceMessageId}, OrderId: {OrderId}, CustomerId: {CustomerId}",
                    sourceMessageId,
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId);

                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<CreateOrderPaidNotificationService>();

                await service.ExecuteAsync(
                    integrationEvent,
                    sourceMessageId,
                    cancellationToken);

                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                _logger.LogInformation(
                    "Order paid event acknowledged. SourceMessageId: {SourceMessageId}, OrderId: {OrderId}",
                    sourceMessageId,
                    integrationEvent.OrderId);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Order paid event rejected because payload is invalid. DeliveryTag: {DeliveryTag}",
                    eventArgs.DeliveryTag);

                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Order paid event rejected because metadata or payload is invalid. DeliveryTag: {DeliveryTag}",
                    eventArgs.DeliveryTag);

                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Order paid event processing failed. DeliveryTag: {DeliveryTag}",
                    eventArgs.DeliveryTag);

                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private void DeclareTopology(IModel channel)
        {
            channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: true,
                autoDelete: false);

            channel.QueueDeclare(
                queue: _options.OrderPaidQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            channel.QueueBind(
                queue: _options.OrderPaidQueueName,
                exchange: _options.ExchangeName,
                routingKey: "orders.paid");
        }

        private static Guid GetSourceMessageId(BasicDeliverEventArgs eventArgs)
        {
            var messageId = eventArgs.BasicProperties?.MessageId;

            if (!Guid.TryParse(messageId, out var sourceMessageId))
                throw new ArgumentException("RabbitMQ message id must be a valid outbox message id.");

            return sourceMessageId;
        }

        private static OrderPaidIntegrationEvent DeserializeEvent(BasicDeliverEventArgs eventArgs)
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.Span);
            var integrationEvent = JsonSerializer.Deserialize<OrderPaidIntegrationEvent>(json, JsonOptions);

            if (integrationEvent is null)
                throw new ArgumentException("Order paid event payload cannot be empty.");

            return integrationEvent;
        }

        private static void Validate(OrderPaidIntegrationEvent integrationEvent)
        {
            if (integrationEvent.OrderId == Guid.Empty)
                throw new ArgumentException("Order paid event order id cannot be empty.");

            if (integrationEvent.CustomerId == Guid.Empty)
                throw new ArgumentException("Order paid event customer id cannot be empty.");

            if (integrationEvent.TotalAmount <= 0)
                throw new ArgumentException("Order paid event total amount must be greater than zero.");

            if (integrationEvent.PaidAtUtc == default)
                throw new ArgumentException("Order paid event paid date cannot be empty.");
        }
    }
}
