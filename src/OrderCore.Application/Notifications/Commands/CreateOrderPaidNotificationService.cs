using Microsoft.Extensions.Logging;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Contracts.Events;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Notifications.Commands
{
    public class CreateOrderPaidNotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<CreateOrderPaidNotificationService> _logger;

        public CreateOrderPaidNotificationService(
            INotificationRepository notificationRepository,
            ILogger<CreateOrderPaidNotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<bool> ExecuteAsync(
            OrderPaidIntegrationEvent integrationEvent,
            Guid sourceMessageId,
            CancellationToken cancellationToken = default)
        {
            var alreadyProcessed = await _notificationRepository.ExistsBySourceMessageIdAsync(
                sourceMessageId,
                cancellationToken);

            if (alreadyProcessed)
            {
                _logger.LogInformation(
                    "Order paid notification already exists. SourceMessageId: {SourceMessageId}, OrderId: {OrderId}",
                    sourceMessageId,
                    integrationEvent.OrderId);

                return false;
            }

            var notification = new Notification(
                NotificationTypes.OrderPaid,
                "Payment confirmed",
                $"Order {integrationEvent.OrderId} was paid. Total amount: {integrationEvent.TotalAmount}.",
                sourceMessageId,
                integrationEvent.OrderId,
                integrationEvent.CustomerId);

            await _notificationRepository.AddAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Order paid notification created. NotificationId: {NotificationId}, SourceMessageId: {SourceMessageId}, OrderId: {OrderId}",
                notification.Id,
                sourceMessageId,
                integrationEvent.OrderId);

            return true;
        }
    }
}
