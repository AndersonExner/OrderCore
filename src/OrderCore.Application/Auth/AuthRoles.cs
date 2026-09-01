namespace OrderCore.Application.Auth;

public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string Sales = "Sales";
    public const string Finance = "Finance";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        Sales,
        Finance,
        Viewer
    };

    public static bool IsValid(string role)
    {
        return All.Contains(role);
    }

    public static string Normalize(string role)
    {
        return All.First(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
    }
}
