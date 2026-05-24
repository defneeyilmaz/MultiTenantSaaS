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
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
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

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tenantSlug = SlugGenerator.From(request.TenantSlug.Trim());

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug, cancellationToken);

        if (tenant is null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email, password, or tenant.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email, password, or tenant.");
        }

        var membership = await _dbContext.UserTenantMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == user.Id && m.TenantId == tenant.Id && m.IsActive,
                cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException("Invalid email, password, or tenant.");
        }

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(
            user,
            tenant,
            membership.Role);

        return new LoginResult(
            accessToken,
            expiresAt,
            tenant.Id,
            tenant.Slug,
            user.Id,
            email,
            membership.Role.ToString());
    }
}
