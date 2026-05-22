using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Auth;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Utilities;

namespace MultiTenantSaaS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<CompanySignupResult> CompanySignupAsync(
        CompanySignupRequest request,
        CancellationToken cancellationToken = default)
    {
        var companyName = request.CompanyName.Trim();
        var email = request.AdminEmail.Trim().ToLowerInvariant();
        var slugSource = string.IsNullOrWhiteSpace(request.CompanySlug)
            ? companyName
            : request.CompanySlug.Trim();
        var slug = SlugGenerator.From(slugSource);

        if (await _dbContext.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            throw new InvalidOperationException($"Tenant slug '{slug}' is already taken.");
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new InvalidOperationException($"Email '{email}' is already registered.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = companyName,
            Slug = slug,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Tenants.Add(tenant);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = request.AdminFullName?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(user, request.AdminPassword);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        _dbContext.UserTenantMemberships.Add(new UserTenantMembership
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = tenant.Id,
            Role = MembershipRole.TenantAdmin,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CompanySignupResult(
            tenant.Id,
            tenant.Slug,
            user.Id,
            email,
            MembershipRole.TenantAdmin.ToString());
    }
}
