using OrderCore.Domain.Entities;

namespace OrderCore.Application.Abstractions.Repositories
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
