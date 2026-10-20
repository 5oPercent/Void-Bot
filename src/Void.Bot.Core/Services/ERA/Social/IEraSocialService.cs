using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Social;

public interface IEraSocialService : IScope
{
    Task<GetEraFriendStatusesResponse> GetEraFriendStatusesAsync(GetEraFriendStatusesRequest request, CancellationToken cancellationToken);
    Task<GetEraFriendTargetResponse> GetEraFriendTargetAsync(GetEraFriendTargetRequest request, CancellationToken cancellationToken);
    Task<JoinEraFriendTeamResponse> JoinEraFriendTeamAsync(JoinEraFriendTeamCommand command, CancellationToken cancellationToken);
    Task<UpdateEraTeamSettingsResponse> UpdateEraTeamSettingsAsync(UpdateEraTeamSettingsCommand command, CancellationToken cancellationToken);
    Task<SendEraTeamInvitationResponse> SendEraTeamInvitationAsync(SendEraTeamInvitationCommand command, CancellationToken cancellationToken);
    Task<GetEraTeamInvitationsResponse> GetEraTeamInvitationsAsync(GetEraTeamInvitationsRequest request, CancellationToken cancellationToken);
    Task<GetEraTeamInvitationResponse> GetEraTeamInvitationAsync(GetEraTeamInvitationRequest request, CancellationToken cancellationToken);
    Task<WithdrawEraTeamInvitationResponse> WithdrawEraTeamInvitationAsync(WithdrawEraTeamInvitationCommand command, CancellationToken cancellationToken);
    Task<RejectEraTeamInvitationResponse> RejectEraTeamInvitationAsync(RejectEraTeamInvitationCommand command, CancellationToken cancellationToken);
    Task<AcceptEraTeamInvitationResponse> AcceptEraTeamInvitationAsync(AcceptEraTeamInvitationCommand command, CancellationToken cancellationToken);
}
