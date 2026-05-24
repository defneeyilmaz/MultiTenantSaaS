namespace MultiTenantSaaS.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MultiTenantSaaS";

    public string Audience { get; set; } = "MultiTenantSaaS";

    public string SecretKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
}
