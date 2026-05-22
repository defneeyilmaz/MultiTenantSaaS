using Microsoft.AspNetCore.Identity;

namespace MultiTenantSaaS.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserTenantMembership> Memberships { get; set; } = [];
}
