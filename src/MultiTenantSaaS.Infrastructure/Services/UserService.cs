using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Application.Contracts.Users;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Security;

namespace MultiTenantSaaS.Infrastructure.Services;

public class UserService : IUserService
{
    private static readonly MembershipRole[] InvitableRoles =
    [
        MembershipRole.TenantAdmin,
        MembershipRole.Manager,
        MembershipRole.Employee
    ];

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<InvitationDto> InviteAsync(
        InviteUserRequest request,
        Guid invitedByUserId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (!Enum.TryParse<MembershipRole>(request.Role.Trim(), true, out var role)
            || !InvitableRoles.Contains(role))
        {
            throw new InvalidOperationException("Invalid invitation role.");
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            var alreadyMember = await _dbContext.UserTenantMemberships
                .AnyAsync(
                    m => m.UserId == existingUser.Id && m.TenantId == tenantId && m.IsActive,
                    cancellationToken);

            if (alreadyMember)
            {
                throw new InvalidOperationException("User is already a member of this tenant.");
            }
        }

        var utcNow = DateTimeOffset.UtcNow;
        var pendingInvitations = await _dbContext.Invitations
            .AsNoTracking()
            .Where(i => i.Email == email && i.AcceptedAt == null)
            .ToListAsync(cancellationToken);

        if (pendingInvitations.Any(i => i.ExpiresAt > utcNow))
        {
            throw new InvalidOperationException("A pending invitation already exists for this email.");
        }

        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            Role = role,
            TokenHash = RefreshTokenHasher.Hash(plainToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            InvitedByUserId = invitedByUserId
        };

        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Invitation token generated for {Email} in tenant {TenantId}. Token: {Token}",
            email,
            tenantId,
            plainToken);

        return new InvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role.ToString(),
            invitation.ExpiresAt);
    }

    public async Task<AcceptInvitationResult> AcceptInvitationAsync(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tokenHash = RefreshTokenHasher.Hash(request.Token.Trim());

        var invitation = await _dbContext.Invitations
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;
        if (invitation is null
            || !string.Equals(invitation.Email, email, StringComparison.OrdinalIgnoreCase)
            || invitation.AcceptedAt is not null
            || invitation.ExpiresAt <= utcNow
            || !invitation.Tenant.IsActive)
        {
            throw new InvalidOperationException("Invalid or expired invitation.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = request.FullName?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }
        else
        {
            var isMember = await _dbContext.UserTenantMemberships
                .AnyAsync(
                    m => m.UserId == user.Id && m.TenantId == invitation.TenantId && m.IsActive,
                    cancellationToken);

            if (isMember)
            {
                throw new InvalidOperationException("User is already a member of this tenant.");
            }
        }

        _dbContext.UserTenantMemberships.Add(new UserTenantMembership
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = invitation.TenantId,
            Role = invitation.Role,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow
        });

        invitation.AcceptedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AcceptInvitationResult(
            user.Id,
            email,
            invitation.TenantId,
            invitation.Tenant.Slug,
            invitation.Role.ToString());
    }

    private Guid GetRequiredTenantId() =>
        _tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is not set.");
}
