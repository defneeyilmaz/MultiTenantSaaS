namespace MultiTenantSaaS.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserTenantMembership> Memberships { get; set; } = [];
}
