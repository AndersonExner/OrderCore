using Microsoft.EntityFrameworkCore;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Domain.Entities;
using OrderCore.Infrastructure.Persistence;

namespace OrderCore.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsBySourceMessageIdAsync(
            Guid sourceMessageId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AnyAsync(x => x.SourceMessageId == sourceMessageId, cancellationToken);
        }

        public async Task<Notification?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Notification>> GetRecentAsync(
            bool unreadOnly,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications.AsQueryable();

            if (unreadOnly)
            {
                query = query.Where(x => x.ReadAtUtc == null);
            }

            return await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
