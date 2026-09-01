namespace OrderCore.Application.Auth.Dtos;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public AuthenticatedUserResponse User { get; set; } = new();
}
