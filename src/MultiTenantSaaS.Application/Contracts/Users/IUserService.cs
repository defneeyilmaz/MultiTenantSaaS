namespace MultiTenantSaaS.Application.Contracts.Users;

public interface IUserService
{
    Task<InvitationDto> InviteAsync(
        InviteUserRequest request,
        Guid invitedByUserId,
        CancellationToken cancellationToken = default);

    Task<AcceptInvitationResult> AcceptInvitationAsync(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken = default);
}
