using OrderCore.Domain.Common;

namespace OrderCore.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ApplicationUser()
    {
    }

    public ApplicationUser(string userName, string email, string passwordHash, string role)
    {
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetRole(role);
        CreatedAtUtc = DateTime.UtcNow;
    }

    private void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.");

        UserName = userName.Trim().ToLowerInvariant();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("User email cannot be empty.");

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid user email.");

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.");

        PasswordHash = passwordHash;
    }

    private void SetRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("User role cannot be empty.");

        Role = role.Trim();
    }
}
