using Microsoft.EntityFrameworkCore;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Domain.Entities;
using OrderCore.Infrastructure.Persistence;

namespace OrderCore.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(x => x.Items)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
    }
}