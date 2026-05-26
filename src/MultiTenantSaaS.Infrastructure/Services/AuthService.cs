using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiTenantSaaS.Application.Contracts.Auth;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Options;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Security;
using MultiTenantSaaS.Shared.Utilities;

namespace MultiTenantSaaS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
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

        var (user, tenant, membership) = await ValidateCredentialsAsync(
            email,
            request.Password,
            tenantSlug,
            cancellationToken);

        var tokens = await IssueTokensAsync(user, tenant, membership.Role, cancellationToken);

        return new LoginResult(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt,
            tokens.TenantId,
            tokens.TenantSlug,
            tokens.UserId,
            tokens.Email,
            tokens.Role);
    }

    public async Task<AuthTokensResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var tokenHash = RefreshTokenHasher.Hash(request.RefreshToken.Trim());
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        if (storedToken.RevokedAt is not null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        if (storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == storedToken.TenantId, cancellationToken);

        if (tenant is null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var membership = await _dbContext.UserTenantMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == user.Id && m.TenantId == tenant.Id && m.IsActive,
                cancellationToken);

        if (membership is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var (plainRefreshToken, newRefreshEntity) = CreateRefreshTokenEntity(user.Id, tenant.Id);
        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.ReplacedByTokenId = newRefreshEntity.Id;

        _dbContext.RefreshTokens.Add(newRefreshEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiresAt) = _jwtTokenGenerator.GenerateAccessToken(
            user,
            tenant,
            membership.Role);

        await transaction.CommitAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken,
            accessExpiresAt,
            plainRefreshToken,
            newRefreshEntity.ExpiresAt,
            tenant.Id,
            tenant.Slug,
            user.Id,
            user.Email ?? string.Empty,
            membership.Role.ToString());
    }

    private async Task<(ApplicationUser User, Tenant Tenant, UserTenantMembership Membership)> ValidateCredentialsAsync(
        string email,
        string password,
        string tenantSlug,
        CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug, cancellationToken);

        if (tenant is null || !tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email, password, or tenant.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
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

        return (user, tenant, membership);
    }

    private async Task<AuthTokensResult> IssueTokensAsync(
        ApplicationUser user,
        Tenant tenant,
        MembershipRole role,
        CancellationToken cancellationToken)
    {
        var (accessToken, accessExpiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, tenant, role);
        var (plainRefreshToken, refreshEntity) = CreateRefreshTokenEntity(user.Id, tenant.Id);

        _dbContext.RefreshTokens.Add(refreshEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken,
            accessExpiresAt,
            plainRefreshToken,
            refreshEntity.ExpiresAt,
            tenant.Id,
            tenant.Slug,
            user.Id,
            user.Email ?? string.Empty,
            role.ToString());
    }

    private (string PlainToken, RefreshToken Entity) CreateRefreshTokenEntity(Guid userId, Guid tenantId)
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var (ipAddress, deviceInfo) = GetClientInfo();

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TokenHash = RefreshTokenHasher.Hash(plainToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo
        };

        return (plainToken, entity);
    }

    private (string? IpAddress, string? DeviceInfo) GetClientInfo()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return (null, null);
        }

        var ip = context.Connection.RemoteIpAddress?.ToString();
        var device = context.Request.Headers.UserAgent.ToString();
        return (ip, string.IsNullOrWhiteSpace(device) ? null : device[..Math.Min(device.Length, 500)]);
    }
}
