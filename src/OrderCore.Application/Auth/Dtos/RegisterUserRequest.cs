using OrderCore.Application.Auth;

namespace OrderCore.Application.Auth.Dtos;

public class RegisterUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = AuthRoles.Viewer;
}
