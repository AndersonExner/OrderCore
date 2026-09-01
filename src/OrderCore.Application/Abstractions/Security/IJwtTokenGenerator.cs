using OrderCore.Domain.Entities;

namespace OrderCore.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) Generate(ApplicationUser user);
}
