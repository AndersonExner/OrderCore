using OrderCore.Domain.Entities;

namespace OrderCore.Application.Abstractions.Repositories;

public interface IApplicationUserRepository
{
    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default);
}
