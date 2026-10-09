using AutoMapper;
using Void.Bot.Core.Settings.ERA;
using System.Linq.Expressions;
using System.Net;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Resources.i18n.Social;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public class EraSocialService : IEraSocialService
{
    private readonly IMapper _mapper;
    private readonly EraNetworkingSetting _setting;
    private readonly IEraTeamDataProvider _teams;
    private readonly IEraDataProvider _players;
    private readonly IEraSocialDataProvider _data;
    private readonly IFriendDataProvider _friends;
    private readonly IEraTeamService _teamService;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localization;

    public EraSocialService(IEraTeamDataProvider teams, IEraDataProvider players,
        IEraSocialDataProvider data, IFriendDataProvider friends, IEraTeamService teamService,
        ICurrentUser currentUser, ILocalizationService localization, IMapper mapper, EraNetworkingSetting setting)
    {
        _mapper = mapper;
        _setting = setting;
        _teams = teams;
        _players = players;
        _data = data;
        _friends = friends;
        _teamService = teamService;
        _currentUser = currentUser;
        _localization = localization;
    }

    private Guid AccountId
    {
        get
        {
            return _currentUser.Id ?? Guid.Empty;
        }
    }

    private T Error<T>(HttpStatusCode status, string code,
        Expression<Func<SocialContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T
        {
            Code = status,
            ErrorCode = code,
            Message = _localization.GetValue(text)
        };
    }

    private async Task<T> GuardAsync<T>(Func<Task<T>> operation) where T : VoidBotResponse, new()
    {
        if (AccountId == Guid.Empty)
        {
            return Error<T>(HttpStatusCode.Unauthorized, "social.unauthorized", text => text.Unauthorized);
        }
        if (!_setting.Enabled)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.networking_disabled", text => text.NetworkingUnavailable);
        }
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
        catch (EraTeamWriteException)
        {
            return Error<T>(HttpStatusCode.Conflict, "era.version_conflict", text => text.TeamUnavailable);
        }
    }

    private static bool IsActive(EraTeam? team)
    {
        var now = DateTimeOffset.UtcNow;
        return team != null && team.ExpiresAt > now && team.Members.Any(value => value.ExpiresAt > now);
    }

    private static EraTeamInvitationDto MapInvitation(EraTeam team, EraTeamInvitation invitation,
        EraTeamInvitationStatus? effectiveStatus = null)
    {
        return new EraTeamInvitationDto
        {
            Id = invitation.Id,
            TeamId = team.TeamId,
            SenderAccountId = invitation.SenderAccountId,
            RecipientAccountId = invitation.RecipientAccountId,
            Status = effectiveStatus ?? invitation.Status,
            Version = invitation.Version,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt
        };
    }

    private async Task<EraTeamInvitationStatus> EffectiveStatusAsync(EraTeam team,
        EraTeamInvitation invitation, CancellationToken cancellationToken)
    {
        if (invitation.Status != EraTeamInvitationStatus.Pending)
        {
            return invitation.Status;
        }
        if (invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return EraTeamInvitationStatus.Expired;
        }
        var sender = team.Members.SingleOrDefault(value => value.MemberId == invitation.SenderMemberId &&
            value.ExpiresAt > DateTimeOffset.UtcNow);
        if (!IsActive(team) || team.JoinMode == EraTeamJoinMode.Solo || sender == null ||
            (team.InvitationPermission == EraTeamInvitationPermission.HostOnly && sender.PlayerId != team.HostPlayerId) ||
            !await _friends.AreFriendsAsync(invitation.SenderAccountId, invitation.RecipientAccountId, cancellationToken).ConfigureAwait(false))
        {
            return EraTeamInvitationStatus.Invalidated;
        }
        var friendship = await _friends.GetFriendshipAsync(invitation.SenderAccountId, invitation.RecipientAccountId,
            cancellationToken).ConfigureAwait(false);
        if (friendship == null || friendship.Id != invitation.FriendshipId)
        {
            return EraTeamInvitationStatus.Invalidated;
        }
        var host = await _data.GetAccountIdAsync(team.HostPlayerId, cancellationToken).ConfigureAwait(false);
        if (!host.HasValue || await _friends.IsBlockedAsync(host.Value, invitation.RecipientAccountId, cancellationToken).ConfigureAwait(false))
        {
            return EraTeamInvitationStatus.Invalidated;
        }
        return EraTeamInvitationStatus.Pending;
    }

    private async Task<EraFriendStatusDto> ReadFriendStatusAsync(Guid friendAccount,
        CancellationToken cancellationToken)
    {
        var statuses = await ReadFriendStatusesBatchAsync(new[] { friendAccount }, cancellationToken)
            .ConfigureAwait(false);
        return statuses.SingleOrDefault() ?? new EraFriendStatusDto
        {
            AccountId = friendAccount
        };
    }

    private async Task<List<EraFriendStatusDto>> ReadFriendStatusesBatchAsync(IReadOnlyCollection<Guid> requested,
        CancellationToken cancellationToken)
    {
        var friends = (await _data.GetFriendAccountIdsAsync(AccountId, cancellationToken).ConfigureAwait(false)).ToHashSet();
        var blocked = (await _friends.GetBlockedAccountIdsAsync(AccountId, cancellationToken).ConfigureAwait(false)).ToHashSet();
        var ids = requested.Distinct().Where(value => friends.Contains(value) && !blocked.Contains(value)).ToArray();
        if (ids.Length == 0)
        {
            return new List<EraFriendStatusDto>();
        }
        var accounts = ids.Append(AccountId).ToArray();
        var players = await _data.GetPlayerIdsAsync(accounts, cancellationToken).ConfigureAwait(false);
        var snapshots = await _teams.GetPresenceSnapshotsAsync(accounts, players, cancellationToken).ConfigureAwait(false);
        var hosts = await _data.GetAccountIdsAsync(snapshots.Where(value => value.Team != null)
            .Select(value => value.Team!.HostPlayerId).Distinct().ToArray(), cancellationToken).ConfigureAwait(false);
        var ownTeam = snapshots.Single(value => value.AccountId == AccountId).Team?.TeamId;
        var result = new List<EraFriendStatusDto>();
        foreach (var snapshot in snapshots.Where(value => value.AccountId != AccountId))
        {
            var status = new EraFriendStatusDto
            {
                AccountId = snapshot.AccountId,
                IsOnline = snapshot.IsOnline
            };
            var team = snapshot.Team;
            if (team != null)
            {
                var summary = new EraFriendTeamDto
                {
                    TeamId = team.TeamId,
                    MemberCount = team.Members.Count(value => value.ExpiresAt > DateTimeOffset.UtcNow),
                    MaxMembers = team.MaxMembers,
                    JoinMode = team.JoinMode
                };
                if (ownTeam == team.TeamId)
                {
                    summary.UnavailableReason = "era.team_already_joined";
                }
                else if (!hosts.TryGetValue(team.HostPlayerId, out var host) || blocked.Contains(host) ||
                    !team.Members.Any(value => value.PlayerId == team.HostPlayerId && value.ExpiresAt > DateTimeOffset.UtcNow))
                {
                    summary.UnavailableReason = "era.team_join_denied";
                }
                else if (team.IsMigrating)
                {
                    summary.UnavailableReason = "era.team_migrating";
                }
                else if (summary.MemberCount >= team.MaxMembers)
                {
                    summary.UnavailableReason = "era.team_capacity";
                }
                else if (team.JoinMode == EraTeamJoinMode.Solo || team.JoinMode == EraTeamJoinMode.InvitationOnly)
                {
                    summary.UnavailableReason = "era.team_join_denied";
                }
                else if (team.JoinMode == EraTeamJoinMode.HostFriends && !friends.Contains(host))
                {
                    summary.UnavailableReason = "era.host_friend_required";
                }
                else
                {
                    summary.CanJoin = true;
                }
                status.Team = summary;
            }
            result.Add(status);
        }
        return result;
    }

    public async Task<GetEraFriendStatusesResponse> GetEraFriendStatusesAsync(GetEraFriendStatusesRequest message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            return new GetEraFriendStatusesResponse
            {
                Data = await ReadFriendStatusesBatchAsync(message.AccountIds, cancellationToken).ConfigureAwait(false)
            };
        }).ConfigureAwait(false);
    }

    public async Task<GetEraFriendTargetResponse> GetEraFriendTargetAsync(GetEraFriendTargetRequest message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            if (!await _friends.AreFriendsAsync(AccountId, message.FriendAccountId, cancellationToken).ConfigureAwait(false))
            {
                return Error<GetEraFriendTargetResponse>(HttpStatusCode.NotFound, "social.account_not_found", text => text.AccountNotFound);
            }
            return new GetEraFriendTargetResponse
            {
                Data = await ReadFriendStatusAsync(message.FriendAccountId, cancellationToken).ConfigureAwait(false)
            };
        }).ConfigureAwait(false);
    }

    public async Task<JoinEraFriendTeamResponse> JoinEraFriendTeamAsync(JoinEraFriendTeamCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var target = await _teams.GetTeamAsync(message.ExpectedTeamId, cancellationToken).ConfigureAwait(false);
            var host = target == null ? null : await _data.GetAccountIdAsync(target.HostPlayerId, cancellationToken).ConfigureAwait(false);
            if (!host.HasValue || !await _friends.LockAccountSetAsync(new[] { AccountId, message.FriendAccountId, host.Value },
                cancellationToken).ConfigureAwait(false) ||
                !await _friends.AreFriendsAsync(AccountId, message.FriendAccountId, cancellationToken).ConfigureAwait(false))
            {
                return Error<JoinEraFriendTeamResponse>(HttpStatusCode.Forbidden, "social.friend_required", text => text.Blocked);
            }
            var status = await ReadFriendStatusAsync(message.FriendAccountId, cancellationToken).ConfigureAwait(false);
            if (status.Team == null || status.Team.TeamId != message.ExpectedTeamId)
            {
                return Error<JoinEraFriendTeamResponse>(HttpStatusCode.Conflict, "era.team_changed", text => text.TeamUnavailable);
            }
            var result = await _teamService.JoinSocialAsync(message.ExpectedTeamId, message.CharacterId,
                message.SourceTeamId, message.SourceMemberId, null, message.FriendAccountId, cancellationToken).ConfigureAwait(false);
            return new JoinEraFriendTeamResponse
            {
                Code = result.Code, ErrorCode = result.ErrorCode, Message = result.Message, Data = result.Data
            };
        }).ConfigureAwait(false);
    }

    public async Task<UpdateEraTeamSettingsResponse> UpdateEraTeamSettingsAsync(UpdateEraTeamSettingsCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var player = await _players.GetPlayerByAccountIdAsync(AccountId, cancellationToken).ConfigureAwait(false);
            return await _teams.ChangeAsync(message.TeamId, team =>
            {
                var now = DateTimeOffset.UtcNow;
                if (!IsActive(team) || player == null || team!.HostPlayerId != player.Id ||
                    !team.Members.Any(value => value.PlayerId == player.Id && value.MemberId == message.MemberId && value.ExpiresAt > now))
                {
                    return new EraTeamChange<UpdateEraTeamSettingsResponse>
                    {
                        Result = Error<UpdateEraTeamSettingsResponse>(HttpStatusCode.Forbidden, "era.team_host_required", text => text.TeamHostRequired)
                    };
                }
                if (!team.IsMigrating && team.SocialVersion == message.ExpectedVersion + 1 &&
                    team.JoinMode == message.JoinMode && team.InvitationPermission == message.InvitationPermission)
                {
                    return new EraTeamChange<UpdateEraTeamSettingsResponse>
                    {
                        Result = new UpdateEraTeamSettingsResponse
                        {
                            Data = MapTeamSummary(team)
                        }
                    };
                }
                if (team.IsMigrating || team.SocialVersion != message.ExpectedVersion)
                {
                    return new EraTeamChange<UpdateEraTeamSettingsResponse>
                    {
                        Result = Error<UpdateEraTeamSettingsResponse>(HttpStatusCode.Conflict, "era.version_conflict", text => text.TeamUnavailable)
                    };
                }
                if (message.JoinMode == EraTeamJoinMode.Solo && team.Members.Count(value => value.ExpiresAt > now) != 1)
                {
                    return new EraTeamChange<UpdateEraTeamSettingsResponse>
                    {
                        Result = Error<UpdateEraTeamSettingsResponse>(HttpStatusCode.Conflict, "era.solo_requires_one_member", text => text.SoloRequiresOneMember)
                    };
                }
                team.JoinMode = message.JoinMode;
                team.InvitationPermission = message.InvitationPermission;
                team.SocialVersion++;
                return new EraTeamChange<UpdateEraTeamSettingsResponse>
                {
                    Changed = true,
                    Team = team,
                    Result = new UpdateEraTeamSettingsResponse
                    {
                        Data = MapTeamSummary(team)
                    }
                };
            }, cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private EraTeamDto MapTeamSummary(EraTeam team)
    {
        var result = _mapper.Map<EraTeamDto>(team);
        result.HeartbeatIntervalSeconds = 5;
        result.MemberLifetimeSeconds = _setting.MemberLifetimeSeconds;
        result.Members.RemoveAll(value => value.ExpiresAt <= DateTimeOffset.UtcNow);
        return result;
    }

    public async Task<SendEraTeamInvitationResponse> SendEraTeamInvitationAsync(SendEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var authorizedTeam = await _teams.GetTeamAsync(message.TeamId, cancellationToken).ConfigureAwait(false);
            var hostAccount = authorizedTeam == null ? null : await _data.GetAccountIdAsync(authorizedTeam.HostPlayerId,
                cancellationToken).ConfigureAwait(false);
            if (message.TargetAccountId == AccountId || !hostAccount.HasValue ||
                !await _friends.LockAccountSetAsync(new[] { AccountId, message.TargetAccountId, hostAccount.Value }, cancellationToken).ConfigureAwait(false) ||
                await _friends.IsBlockedAsync(hostAccount.Value, message.TargetAccountId, cancellationToken).ConfigureAwait(false) ||
                !await _friends.AreFriendsAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false))
            {
                return Error<SendEraTeamInvitationResponse>(HttpStatusCode.Forbidden, "social.friend_required", text => text.Blocked);
            }
            var player = await _players.GetPlayerByAccountIdAsync(AccountId, cancellationToken).ConfigureAwait(false);
            if (player == null)
            {
                return Error<SendEraTeamInvitationResponse>(HttpStatusCode.NotFound, "era.player_not_found", text => text.AccountNotFound);
            }
            var friendship = await _friends.GetFriendshipAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false);
            return await _teams.ChangeAsync(message.TeamId, team =>
            {
                var now = DateTimeOffset.UtcNow;
                var sender = team?.Members.SingleOrDefault(value => value.PlayerId == player.Id &&
                    value.MemberId == message.MemberId && value.ExpiresAt > now);
                if (!IsActive(team) || sender == null || team!.HostPlayerId != authorizedTeam!.HostPlayerId || team.IsMigrating || team.JoinMode == EraTeamJoinMode.Solo ||
                    (team.InvitationPermission == EraTeamInvitationPermission.HostOnly && team.HostPlayerId != player.Id))
                {
                    return new EraTeamChange<SendEraTeamInvitationResponse>
                    {
                        Result = Error<SendEraTeamInvitationResponse>(HttpStatusCode.Forbidden, "era.invitation_denied", text => text.TeamUnavailable)
                    };
                }
                var receipt = team.Invitations.SingleOrDefault(value => value.SenderAccountId == AccountId && value.RequestId == message.RequestId);
                if (receipt != null)
                {
                    return new EraTeamChange<SendEraTeamInvitationResponse>
                    {
                        Result = receipt.RecipientAccountId == message.TargetAccountId
                            ? new SendEraTeamInvitationResponse
                            {
                                Data = MapInvitation(team, receipt)
                            }
                            : Error<SendEraTeamInvitationResponse>(HttpStatusCode.Conflict, "social.idempotency_conflict", text => text.IdempotencyConflict)
                    };
                }
                var existing = team.Invitations.SingleOrDefault(value => value.SenderAccountId == AccountId &&
                    value.RecipientAccountId == message.TargetAccountId && value.Status == EraTeamInvitationStatus.Pending && value.ExpiresAt > now);
                if (existing != null)
                {
                    return new EraTeamChange<SendEraTeamInvitationResponse>
                    {
                        Result = new SendEraTeamInvitationResponse
                        {
                            Data = MapInvitation(team, existing)
                        }
                    };
                }
                team.Invitations.RemoveAll(value => value.ExpiresAt.AddMinutes(10) <= now);
                if (team.Invitations.Count >= 128 || team.Invitations.Count(value => value.SenderAccountId == AccountId &&
                    value.CreatedAt > now.AddMinutes(-1)) >= 10)
                {
                    return new EraTeamChange<SendEraTeamInvitationResponse>
                    {
                        Result = Error<SendEraTeamInvitationResponse>(HttpStatusCode.TooManyRequests, "social.limit_reached", text => text.LimitReached)
                    };
                }
                var invitation = new EraTeamInvitation
                {
                    Id = Guid.NewGuid(),
                    FriendshipId = friendship!.Id,
                    RequestId = message.RequestId,
                    SenderAccountId = AccountId,
                    RecipientAccountId = message.TargetAccountId,
                    SenderMemberId = sender.MemberId,
                    Status = EraTeamInvitationStatus.Pending,
                    CreatedAt = now,
                    ExpiresAt = now.AddSeconds(120)
                };
                team.Invitations.Add(invitation);
                return new EraTeamChange<SendEraTeamInvitationResponse>
                {
                    Changed = true,
                    Team = team,
                    Result = new SendEraTeamInvitationResponse
                    {
                        Data = MapInvitation(team, invitation)
                    }
                };
            }, cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<GetEraTeamInvitationsResponse> GetEraTeamInvitationsAsync(GetEraTeamInvitationsRequest message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var ids = await _teams.GetInvitationTeamIdsAsync(AccountId, cancellationToken).ConfigureAwait(false);
            var result = new List<EraTeamInvitationDto>();
            foreach (var id in ids)
            {
                var team = await _teams.GetTeamAsync(id, cancellationToken).ConfigureAwait(false);
                if (team == null)
                {
                    continue;
                }
                foreach (var invitation in team.Invitations.Where(value =>
                    message.Outgoing ? value.SenderAccountId == AccountId : value.RecipientAccountId == AccountId))
                {
                    var status = await EffectiveStatusAsync(team, invitation, cancellationToken).ConfigureAwait(false);
                    if (status == EraTeamInvitationStatus.Pending)
                    {
                        result.Add(MapInvitation(team, invitation, status));
                    }
                }
            }
            return new GetEraTeamInvitationsResponse
            {
                Data = result.OrderByDescending(value => value.CreatedAt).Take(100).ToList()
            };
        }).ConfigureAwait(false);
    }

    public async Task<GetEraTeamInvitationResponse> GetEraTeamInvitationAsync(GetEraTeamInvitationRequest message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var team = await _teams.GetTeamAsync(message.TeamId, cancellationToken).ConfigureAwait(false);
            var invitation = team?.Invitations.SingleOrDefault(value => value.Id == message.InvitationId &&
                (value.SenderAccountId == AccountId || value.RecipientAccountId == AccountId));
            if (team == null || invitation == null)
            {
                return Error<GetEraTeamInvitationResponse>(HttpStatusCode.NotFound, "era.invitation_not_found", text => text.TeamUnavailable);
            }
            return new GetEraTeamInvitationResponse
            {
                Data = MapInvitation(team, invitation, await EffectiveStatusAsync(team, invitation, cancellationToken).ConfigureAwait(false))
            };
        }).ConfigureAwait(false);
    }

    private async Task<VoidBotResponse<bool>> ProcessInvitationAsync(Guid teamId, Guid invitationId,
        bool withdraw, CancellationToken cancellationToken)
    {
        return await _teams.ChangeAsync(teamId, team =>
        {
            var invitation = team?.Invitations.SingleOrDefault(value => value.Id == invitationId &&
                (withdraw ? value.SenderAccountId : value.RecipientAccountId) == AccountId);
            if (invitation == null || team == null)
            {
                return new EraTeamChange<VoidBotResponse<bool>>
                {
                    Result = Error<VoidBotResponse<bool>>(HttpStatusCode.NotFound, "era.invitation_not_found", text => text.TeamUnavailable)
                };
            }
            var status = withdraw ? EraTeamInvitationStatus.Withdrawn : EraTeamInvitationStatus.Rejected;
            if (invitation.Status == status)
            {
                return new EraTeamChange<VoidBotResponse<bool>>
                {
                    Result = new VoidBotResponse<bool>
                    {
                        Data = true
                    }
                };
            }
            if (invitation.Status != EraTeamInvitationStatus.Pending || invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return new EraTeamChange<VoidBotResponse<bool>>
                {
                    Result = Error<VoidBotResponse<bool>>(HttpStatusCode.Conflict, "era.invitation_invalid", text => text.TeamUnavailable)
                };
            }
            invitation.Status = status;
            invitation.ProcessedAt = DateTimeOffset.UtcNow;
            invitation.Version++;
            return new EraTeamChange<VoidBotResponse<bool>>
            {
                Changed = true,
                Team = team,
                Result = new VoidBotResponse<bool>
                {
                    Data = true
                }
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WithdrawEraTeamInvitationResponse> WithdrawEraTeamInvitationAsync(WithdrawEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var result = await ProcessInvitationAsync(message.TeamId, message.InvitationId, true, cancellationToken).ConfigureAwait(false);
            return new WithdrawEraTeamInvitationResponse
            {
                Code = result.Code,
                ErrorCode = result.ErrorCode,
                Message = result.Message,
                Data = result.Data
            };
        }).ConfigureAwait(false);
    }

    public async Task<RejectEraTeamInvitationResponse> RejectEraTeamInvitationAsync(RejectEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var result = await ProcessInvitationAsync(message.TeamId, message.InvitationId, false, cancellationToken).ConfigureAwait(false);
            return new RejectEraTeamInvitationResponse
            {
                Code = result.Code,
                ErrorCode = result.ErrorCode,
                Message = result.Message,
                Data = result.Data
            };
        }).ConfigureAwait(false);
    }

    public async Task<AcceptEraTeamInvitationResponse> AcceptEraTeamInvitationAsync(AcceptEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        return await GuardAsync(async () =>
        {
            var team = await _teams.GetTeamAsync(message.TeamId, cancellationToken).ConfigureAwait(false);
            var invitation = team?.Invitations.SingleOrDefault(value => value.Id == message.InvitationId && value.RecipientAccountId == AccountId);
            if (team == null || invitation == null)
            {
                return Error<AcceptEraTeamInvitationResponse>(HttpStatusCode.NotFound, "era.invitation_not_found", text => text.TeamUnavailable);
            }
            if (invitation.Status == EraTeamInvitationStatus.Accepted)
            {
                var player = await _players.GetPlayerByAccountIdAsync(AccountId, cancellationToken).ConfigureAwait(false);
                var member = team.Members.SingleOrDefault(value => value.MemberId == invitation.AcceptedMemberId &&
                    value.PlayerId == player?.Id && value.ExpiresAt > DateTimeOffset.UtcNow);
                if (member != null && invitation.AcceptedCharacterId == message.CharacterId)
                {
                    return new AcceptEraTeamInvitationResponse
                    {
                        Data = MapTeamSummary(team)
                    };
                }
                return Error<AcceptEraTeamInvitationResponse>(HttpStatusCode.Conflict, "era.invitation_consumed", text => text.TeamUnavailable);
            }
            if (await EffectiveStatusAsync(team, invitation, cancellationToken).ConfigureAwait(false) != EraTeamInvitationStatus.Pending)
            {
                return Error<AcceptEraTeamInvitationResponse>(HttpStatusCode.Conflict, "era.invitation_invalid", text => text.TeamUnavailable);
            }
            var result = await _teamService.JoinSocialAsync(message.TeamId, message.CharacterId,
                message.SourceTeamId, message.SourceMemberId, message.InvitationId, null, cancellationToken).ConfigureAwait(false);
            return new AcceptEraTeamInvitationResponse
            {
                Code = result.Code, ErrorCode = result.ErrorCode, Message = result.Message, Data = result.Data
            };
        }).ConfigureAwait(false);
    }
}
