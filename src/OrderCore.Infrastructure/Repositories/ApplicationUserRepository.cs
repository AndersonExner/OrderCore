using Microsoft.EntityFrameworkCore;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Domain.Entities;
using OrderCore.Infrastructure.Persistence;

namespace OrderCore.Infrastructure.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly AppDbContext _dbContext;

    public ApplicationUserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApplicationUsers.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToLowerInvariant();

        return await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(x => x.UserName.ToLower() == normalizedUserName, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByUserNameOrEmailAsync(
        string userNameOrEmail,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = userNameOrEmail.Trim().ToLowerInvariant();

        return await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x => x.Email == normalizedIdentifier || x.UserName.ToLower() == normalizedIdentifier,
                cancellationToken);
    }
}
