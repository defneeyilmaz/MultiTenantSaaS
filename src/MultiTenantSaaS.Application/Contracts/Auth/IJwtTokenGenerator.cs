using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;

namespace MultiTenantSaaS.Application.Contracts.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(
        ApplicationUser user,
        Tenant tenant,
        MembershipRole role,
        IReadOnlyList<string> permissions);
}
