using Void.Bot.Message.Dto;
using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraTeamService
{
    Task<VoidBotResponse<EraTeamDto>> JoinSocialAsync(Guid targetTeamId, Guid characterId,
        Guid? sourceTeamId, Guid? sourceMemberId, Guid? invitationId, Guid? requiredFriendAccountId, CancellationToken cancellationToken);
}

public partial class EraTeamService
{
    private readonly IFriendDataProvider _friends;
    private readonly IEraSocialDataProvider _socialData;

    private async Task<bool> CanJoinSocialAsync(EraTeam team, Guid? invitationId,
        bool lockAccounts, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return false;
        }
        var accountId = _currentUser.Id.Value;
        var now = DateTimeOffset.UtcNow;
        if (team.ExpiresAt <= now || !team.Members.Any(value => value.PlayerId == team.HostPlayerId && value.ExpiresAt > now))
        {
            return false;
        }
        var hostAccount = await _socialData.GetAccountIdAsync(team.HostPlayerId, cancellationToken).ConfigureAwait(false);
        if (!hostAccount.HasValue)
        {
            return false;
        }
        var accounts = new List<Guid> { accountId, hostAccount.Value };
        if (invitationId.HasValue)
        {
            var sponsor = team.Invitations.SingleOrDefault(value => value.Id == invitationId.Value);
            if (sponsor != null)
            {
                accounts.Add(sponsor.SenderAccountId);
            }
        }
        if (lockAccounts && !await _friends.LockAccountSetAsync(accounts,
            cancellationToken).ConfigureAwait(false))
        {
            return false;
        }
        if (team.JoinMode == EraTeamJoinMode.Solo ||
            await _friends.IsBlockedAsync(accountId, hostAccount.Value, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }
        if (invitationId.HasValue)
        {
            var invitation = team.Invitations.SingleOrDefault(value => value.Id == invitationId.Value);
            if (invitation == null || invitation.RecipientAccountId != accountId ||
                invitation.Status != EraTeamInvitationStatus.Pending || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return false;
            }
            var sender = team.Members.SingleOrDefault(value => value.MemberId == invitation.SenderMemberId &&
                value.ExpiresAt > DateTimeOffset.UtcNow);
            if (sender == null || (team.InvitationPermission == EraTeamInvitationPermission.HostOnly &&
                sender.PlayerId != team.HostPlayerId))
            {
                return false;
            }
            var friendship = await _friends.GetFriendshipAsync(accountId, invitation.SenderAccountId,
                cancellationToken).ConfigureAwait(false);
            if (friendship == null || friendship.Id != invitation.FriendshipId)
            {
                return false;
            }
            return await _friends.AreFriendsAsync(accountId, invitation.SenderAccountId,
                cancellationToken).ConfigureAwait(false);
        }
        if (team.JoinMode == EraTeamJoinMode.Public)
        {
            return true;
        }
        if (team.JoinMode == EraTeamJoinMode.HostFriends)
        {
            return await _friends.AreFriendsAsync(accountId, hostAccount.Value, cancellationToken).ConfigureAwait(false);
        }
        return false;
    }

    private static bool SameJoinPolicy(EraTeam team, EraTeam authorized)
    {
        return team.HostPlayerId == authorized.HostPlayerId && team.SocialVersion == authorized.SocialVersion &&
            team.JoinMode == authorized.JoinMode && team.InvitationPermission == authorized.InvitationPermission;
    }

    private static bool ConsumeInvitation(EraTeam team, Guid? invitationId, Guid accountId,
        EraTeamMember member, DateTimeOffset now)
    {
        if (!invitationId.HasValue)
        {
            return true;
        }
        var invitation = team.Invitations.SingleOrDefault(value => value.Id == invitationId.Value &&
            value.RecipientAccountId == accountId);
        if (invitation == null || invitation.Status != EraTeamInvitationStatus.Pending || invitation.ExpiresAt <= now)
        {
            return false;
        }
        var sender = team.Members.SingleOrDefault(value => value.MemberId == invitation.SenderMemberId && value.ExpiresAt > now);
        if (sender == null || (team.InvitationPermission == EraTeamInvitationPermission.HostOnly &&
            sender.PlayerId != team.HostPlayerId))
        {
            return false;
        }
        invitation.Status = EraTeamInvitationStatus.Accepted;
        invitation.AcceptedMemberId = member.MemberId;
        invitation.AcceptedCharacterId = member.CharacterId;
        invitation.ProcessedAt = now;
        invitation.Version++;
        return true;
    }

    public async Task<VoidBotResponse<EraTeamDto>> JoinSocialAsync(Guid targetTeamId, Guid characterId,
        Guid? sourceTeamId, Guid? sourceMemberId, Guid? invitationId, Guid? requiredFriendAccountId, CancellationToken cancellationToken)
    {
        Guid? requiredFriendPlayerId = null;
        if (requiredFriendAccountId.HasValue)
        {
            var friend = await _players.GetPlayerByAccountIdAsync(requiredFriendAccountId.Value,
                cancellationToken).ConfigureAwait(false);
            if (friend == null)
            {
                return new VoidBotResponse<EraTeamDto>
                {
                    Code = HttpStatusCode.NotFound,
                    ErrorCode = "era.team_changed"
                };
            }
            requiredFriendPlayerId = friend.Id;
        }
        var target = await _teams.GetTeamAsync(targetTeamId, cancellationToken).ConfigureAwait(false);
        if (target == null)
        {
            return new VoidBotResponse<EraTeamDto>
            {
                Code = HttpStatusCode.NotFound,
                ErrorCode = "era.team_not_found"
            };
        }
        if (sourceTeamId.HasValue)
        {
            var result = await SwitchCoreAsync(new SwitchEraTeamCommand
            {
                TeamId = sourceTeamId.Value,
                MemberId = sourceMemberId!.Value,
                CharacterId = characterId,
                TeamCode = target.TeamCode
            }, invitationId, targetTeamId, cancellationToken, requiredFriendPlayerId).ConfigureAwait(false);
            return new VoidBotResponse<EraTeamDto>
            {
                Code = result.Code, ErrorCode = result.ErrorCode, Message = result.Message, Data = result.Data
            };
        }
        var joined = await JoinCoreAsync(new JoinEraTeamCommand
        {
            CharacterId = characterId,
            TeamCode = target.TeamCode
        }, cancellationToken, invitationId, targetTeamId, requiredFriendPlayerId).ConfigureAwait(false);
        return new VoidBotResponse<EraTeamDto>
        {
            Code = joined.Code, ErrorCode = joined.ErrorCode, Message = joined.Message, Data = joined.Data
        };
    }
}
