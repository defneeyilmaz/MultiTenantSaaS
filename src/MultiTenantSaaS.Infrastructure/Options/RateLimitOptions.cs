namespace MultiTenantSaaS.Infrastructure.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int LoginPermitLimit { get; set; } = 10;

    public int LoginWindowSeconds { get; set; } = 60;

    public int RefreshPermitLimit { get; set; } = 20;

    public int RefreshWindowSeconds { get; set; } = 60;
}
