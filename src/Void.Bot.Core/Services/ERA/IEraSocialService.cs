using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraSocialService : IScope
{
    Task<GetEraFriendStatusesResponse> GetEraFriendStatusesAsync(GetEraFriendStatusesRequest message, CancellationToken cancellationToken);
    Task<GetEraFriendTargetResponse> GetEraFriendTargetAsync(GetEraFriendTargetRequest message, CancellationToken cancellationToken);
    Task<JoinEraFriendTeamResponse> JoinEraFriendTeamAsync(JoinEraFriendTeamCommand message, CancellationToken cancellationToken);
    Task<UpdateEraTeamSettingsResponse> UpdateEraTeamSettingsAsync(UpdateEraTeamSettingsCommand message, CancellationToken cancellationToken);
    Task<SendEraTeamInvitationResponse> SendEraTeamInvitationAsync(SendEraTeamInvitationCommand message, CancellationToken cancellationToken);
    Task<GetEraTeamInvitationsResponse> GetEraTeamInvitationsAsync(GetEraTeamInvitationsRequest message, CancellationToken cancellationToken);
    Task<GetEraTeamInvitationResponse> GetEraTeamInvitationAsync(GetEraTeamInvitationRequest message, CancellationToken cancellationToken);
    Task<WithdrawEraTeamInvitationResponse> WithdrawEraTeamInvitationAsync(WithdrawEraTeamInvitationCommand message, CancellationToken cancellationToken);
    Task<RejectEraTeamInvitationResponse> RejectEraTeamInvitationAsync(RejectEraTeamInvitationCommand message, CancellationToken cancellationToken);
    Task<AcceptEraTeamInvitationResponse> AcceptEraTeamInvitationAsync(AcceptEraTeamInvitationCommand message, CancellationToken cancellationToken);
}
