using OrderCore.Application.Abstractions.Repositories;
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
    }
}
