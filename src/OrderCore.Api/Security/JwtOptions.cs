namespace OrderCore.Api.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "OrderCore";
    public string Audience { get; set; } = "OrderCore.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
}
