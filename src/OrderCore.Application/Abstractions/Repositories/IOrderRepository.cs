using OrderCore.Domain.Entities;

namespace OrderCore.Application.Abstractions.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
