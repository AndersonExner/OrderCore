using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Exceptions;
using OrderCore.Application.Notifications.Dtos;

namespace OrderCore.Application.Notifications.Commands
{
    public class MarkNotificationAsReadService
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkNotificationAsReadService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<NotificationResponse> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);

            if (notification is null)
                throw new NotFoundException("Notification not found.");

            notification.MarkAsRead();

            await _notificationRepository.SaveChangesAsync(cancellationToken);

            return NotificationMapper.ToResponse(notification);
        }
    }
}
