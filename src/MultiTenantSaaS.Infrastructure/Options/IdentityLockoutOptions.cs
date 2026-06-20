namespace MultiTenantSaaS.Infrastructure.Options;

public class IdentityLockoutOptions
{
    public const string SectionName = "Identity";

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
