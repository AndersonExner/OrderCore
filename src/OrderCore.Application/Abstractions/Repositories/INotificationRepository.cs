using OrderCore.Domain.Entities;

namespace OrderCore.Application.Abstractions.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

        Task<bool> ExistsBySourceMessageIdAsync(
            Guid sourceMessageId,
            CancellationToken cancellationToken = default);

        Task<Notification?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Notification>> GetRecentAsync(
            bool unreadOnly,
            int limit,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
