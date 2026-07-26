using OrderCore.Application.Notifications.Dtos;
using OrderCore.Domain.Entities;

namespace OrderCore.Application.Notifications
{
    internal static class NotificationMapper
    {
        public static NotificationResponse ToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                SourceMessageId = notification.SourceMessageId,
                OrderId = notification.OrderId,
                CustomerId = notification.CustomerId,
                CreatedAtUtc = notification.CreatedAtUtc,
                ReadAtUtc = notification.ReadAtUtc,
                IsRead = notification.IsRead
            };
        }
    }
}
