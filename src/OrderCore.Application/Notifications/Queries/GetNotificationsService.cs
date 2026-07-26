using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Notifications.Dtos;

namespace OrderCore.Application.Notifications.Queries
{
    public class GetNotificationsService
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationsService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IReadOnlyList<NotificationResponse>> ExecuteAsync(
            bool unreadOnly,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var safeLimit = Math.Clamp(limit, 1, 50);
            var notifications = await _notificationRepository.GetRecentAsync(
                unreadOnly,
                safeLimit,
                cancellationToken);

            return notifications
                .Select(NotificationMapper.ToResponse)
                .ToList();
        }
    }
}
