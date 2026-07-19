using Microsoft.EntityFrameworkCore;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Application.Common.Outbox;
using OrderCore.Infrastructure.Persistence;

namespace OrderCore.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly AppDbContext _context;

        public OutboxRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            string type,
            string payload,
            CancellationToken cancellationToken = default)
        {
            var message = new OutboxMessage(type, payload);

            await _context.OutboxMessages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessageEnvelope>> GetPendingAsync(
            int batchSize,
            int maxRetryCount,
            CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages
                .Where(x =>
                    x.Status == OutboxMessageStatus.Pending ||
                    (x.Status == OutboxMessageStatus.Failed && x.RetryCount < maxRetryCount))
                .OrderBy(x => x.CreatedAtUtc)
                .Take(batchSize)
                .Select(x => new OutboxMessageEnvelope(
                    x.Id,
                    x.Type,
                    x.Payload,
                    x.RetryCount,
                    x.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsProcessedAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var message = await GetMessageByIdAsync(id, cancellationToken);

            message.MarkAsProcessed();

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAsFailedAsync(
            Guid id,
            string error,
            CancellationToken cancellationToken = default)
        {
            var message = await GetMessageByIdAsync(id, cancellationToken);

            message.MarkAsFailed(error);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<OutboxMessage> GetMessageByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var message = await _context.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (message is null)
                throw new InvalidOperationException($"Outbox message {id} not found.");

            return message;
        }
    }
}
