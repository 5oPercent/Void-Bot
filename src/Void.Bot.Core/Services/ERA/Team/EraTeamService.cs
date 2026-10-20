using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Core.Services.Social;
using StackExchange.Redis;
using Void.Bot.Message.Constants.ERA;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Team;

public partial interface IEraTeamService : IScope
{
    Task<CreateEraTeamResponse> CreateAsync(
        CreateEraTeamCommand command,
        CancellationToken cancellationToken);
    Task<JoinEraTeamResponse> JoinAsync(
        JoinEraTeamCommand command,
        CancellationToken cancellationToken);
    Task<HeartbeatEraTeamResponse> HeartbeatAsync(
        HeartbeatEraTeamCommand command,
        CancellationToken cancellationToken);
    Task<LeaveEraTeamResponse> LeaveAsync(
        LeaveEraTeamCommand command,
        CancellationToken cancellationToken);
    Task<PrepareEraPunchResponse> PreparePunchAsync(
        PrepareEraPunchCommand command,
        CancellationToken cancellationToken);
    Task<GetCurrentEraTeamResponse> GetCurrentAsync(
        GetCurrentEraTeamRequest request,
        CancellationToken cancellationToken);
    Task<GetEraTeamResponse> GetAsync(
        GetEraTeamRequest request,
        CancellationToken cancellationToken);
    Task<GetEraPunchPeersResponse> GetPeersAsync(
        GetEraPunchPeersRequest request,
        CancellationToken cancellationToken);
}

public partial class EraTeamService : IEraTeamService
{
    private readonly IEraDataProvider _players;
    private readonly IEraTeamDataProvider _teams;
    private readonly IEraPlayerContext _playerContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localization;
    private readonly IMapper _mapper;
    private readonly EraNetworkingSetting _setting;

    public EraTeamService(
        IEraDataProvider players,
        IEraTeamDataProvider teams,
        IEraPlayerContext playerContext,
        ICurrentUser currentUser,
        ILocalizationService localization,
        IMapper mapper,
        EraNetworkingSetting setting,
        IFriendDataProvider friends,
        IEraSocialDataProvider socialData)
    {
        _players = players;
        _teams = teams;
        _playerContext = playerContext;
        _currentUser = currentUser;
        _localization = localization;
        _mapper = mapper;
        _setting = setting;
        _friends = friends;
        _socialData = socialData;
    }

    private T Error<T>(HttpStatusCode status, string code, Expression<Func<EraContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T
        {
            Code = status,
            ErrorCode = code,
            Message = _localization.GetValue(text)
        };
    }

    private async Task<(Guid? PlayerId, T? Error)> IdentifyAsync<T>(CancellationToken cancellationToken)
        where T : VoidBotResponse, new()
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return (null, Error<T>(HttpStatusCode.Unauthorized, "era.unauthorized", text => text.Unauthorized));
        }

        if (!_setting.Enabled)
        {
            return (null, Error<T>(HttpStatusCode.ServiceUnavailable, "era.networking_disabled", text => text.NetworkingDisabled));
        }

        var player = await _playerContext.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);

        return (player.Id, null);
    }

    private async Task<bool> OwnsCharacterAsync(Guid playerId, Guid characterId, CancellationToken cancellationToken)
    {
        var character = await _players.GetCharacterByIdAsync(characterId, cancellationToken).ConfigureAwait(false);
        return character != null && character.PlayerId == playerId && !character.IsDisabled;
    }

    private static bool IsActive(EraTeam team, DateTimeOffset now)
    {
        return team.ExpiresAt > now && team.Members.Any(
            member => member.ExpiresAt > now);
    }

    private static string GenerateTeamCode()
    {
        var characters = new char[EraNetworkingConstants.TeamCodeLength];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = EraNetworkingConstants.TeamCodeAlphabet[
                RandomNumberGenerator.GetInt32(EraNetworkingConstants.TeamCodeAlphabet.Length)];
        }
        return new string(characters);
    }

    private EraTeamMember SetMemberAccount(EraTeamMember member)
    {
        // JWT 的 Name claim 保存主账号完整显示名，不接受客户端自行提交的名字。
        var displayName = _currentUser.UserName;
        var separator = displayName.LastIndexOf('#');
        if (separator > 0 && displayName.Length - separator - 1 == 4 &&
            displayName[(separator + 1)..].All(character => character >= '0' && character <= '9'))
        {
            member.UserName = displayName[..separator];
            member.Suffix = displayName[(separator + 1)..];
        }
        else
        {
            member.UserName = displayName;
            member.Suffix = string.Empty;
        }
        return member;
    }

    private EraTeamDto MapTeam(EraTeam team, DateTimeOffset now)
    {
        var result = _mapper.Map<EraTeamDto>(team);
        result.HeartbeatIntervalSeconds = 5;
        result.MemberLifetimeSeconds = _setting.MemberLifetimeSeconds;
        result.Members.RemoveAll(member => member.ExpiresAt <= now);
        return result;
    }

    public async Task<CreateEraTeamResponse> CreateAsync(CreateEraTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await CreateCoreAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<CreateEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<CreateEraTeamResponse> CreateCoreAsync(CreateEraTeamCommand message, CancellationToken cancellationToken)
    {
        var identity = await IdentifyAsync<CreateEraTeamResponse>(cancellationToken).ConfigureAwait(false);
        if (identity.Error != null)
        {
            return identity.Error;
        }

        if (!await OwnsCharacterAsync(identity.PlayerId!.Value, message.CharacterId, cancellationToken).ConfigureAwait(false))
        {
            return Error<CreateEraTeamResponse>(HttpStatusCode.NotFound, "era.character_not_found", text => text.CharacterNotFound);
        }

        for (var attempt = 0; attempt < 8; attempt++)
        {
            var existingId = await _teams.GetPlayerTeamIdAsync(identity.PlayerId.Value, cancellationToken).ConfigureAwait(false);
            if (existingId.HasValue)
            {
                var existing = await _teams.GetTeamAsync(existingId.Value, cancellationToken).ConfigureAwait(false);
                var now = DateTimeOffset.UtcNow;
                if (existing != null && IsActive(existing, now))
                {
                    var host = existing.Members.SingleOrDefault(member =>
                        member.PlayerId == identity.PlayerId.Value && member.ExpiresAt > now);
                    if (existing.HostPlayerId == identity.PlayerId.Value &&
                        existing.CreationRequestId == message.RequestId &&
                        existing.JoinMode == message.JoinMode && existing.InvitationPermission == message.InvitationPermission &&
                        host != null && host.CharacterId == message.CharacterId)
                    {
                        return new CreateEraTeamResponse
                        {
                            Data = MapTeam(existing, now)
                        };
                    }

                    return Error<CreateEraTeamResponse>(HttpStatusCode.Conflict, "era.team_already_joined", text => text.TeamAlreadyJoined);
                }
            }

            var createdAt = DateTimeOffset.UtcNow;
            var team = new EraTeam
            {
                TeamId = Guid.NewGuid(),
                TeamCode = GenerateTeamCode(),
                HostPlayerId = identity.PlayerId.Value,
                CreationRequestId = message.RequestId,
                JoinMode = message.JoinMode,
                InvitationPermission = message.InvitationPermission,
                MaxMembers = _setting.MaxMembers,
                ExpiresAt = createdAt.AddSeconds(_setting.TeamLifetimeSeconds)
            };
            team.Members.Add(SetMemberAccount(new EraTeamMember
            {
                MemberId = Guid.NewGuid(),
                JoinedAt = createdAt,
                CanHost = true,
                PlayerId = identity.PlayerId.Value,
                CharacterId = message.CharacterId,
                ExpiresAt = createdAt.AddSeconds(_setting.MemberLifetimeSeconds)
            }));
            var status = await _teams.TryCreateAsync(team, cancellationToken).ConfigureAwait(false);
            if (status == EraTeamWriteStatus.Success)
            {
                return new CreateEraTeamResponse
                {
                    Data = MapTeam(team, createdAt)
                };
            }
        }

        return Error<CreateEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
    }

    public async Task<JoinEraTeamResponse> JoinAsync(JoinEraTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await JoinCoreAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<JoinEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<JoinEraTeamResponse> JoinCoreAsync(JoinEraTeamCommand message, CancellationToken cancellationToken,
        Guid? invitationId = null, Guid? expectedTeamId = null, Guid? requiredFriendPlayerId = null)
    {
        var identity = await IdentifyAsync<JoinEraTeamResponse>(cancellationToken).ConfigureAwait(false);
        if (identity.Error != null)
        {
            return identity.Error;
        }

        if (!await OwnsCharacterAsync(identity.PlayerId!.Value, message.CharacterId, cancellationToken).ConfigureAwait(false))
        {
            return Error<JoinEraTeamResponse>(HttpStatusCode.NotFound, "era.character_not_found", text => text.CharacterNotFound);
        }

        var teamCode = message.TeamCode.ToUpperInvariant();
        var teamId = await _teams.GetTeamIdByCodeAsync(teamCode, cancellationToken).ConfigureAwait(false);
        if (!teamId.HasValue)
        {
            return Error<JoinEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
        }
        if (expectedTeamId.HasValue && teamId.Value != expectedTeamId.Value)
        {
            return Error<JoinEraTeamResponse>(HttpStatusCode.Conflict, "era.team_changed", text => text.TeamJoinDenied);
        }
        var authorizedTeam = await _teams.GetTeamAsync(teamId.Value, cancellationToken).ConfigureAwait(false);
        if (authorizedTeam == null)
        {
            return Error<JoinEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
        }
        var canJoin = await CanJoinSocialAsync(authorizedTeam, invitationId, true, cancellationToken).ConfigureAwait(false);
        return await ChangeTeamAsync(teamId.Value, team =>
        {
            var now = DateTimeOffset.UtcNow;
            if (team == null || team.TeamCode != teamCode || !IsActive(team, now) ||
                (requiredFriendPlayerId.HasValue && !team.Members.Any(value =>
                    value.PlayerId == requiredFriendPlayerId.Value && value.ExpiresAt > now)))
            {
                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = Error<JoinEraTeamResponse>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound)
                };
            }

            if (team.IsMigrating)
            {
                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = Error<JoinEraTeamResponse>(HttpStatusCode.Conflict, "era.team_migrating", text => text.TeamMigrating)
                };
            }
            var member = team.Members.SingleOrDefault(item => item.PlayerId == identity.PlayerId.Value && item.ExpiresAt > now);
            if (member != null)
            {
                var invitationReceipt = invitationId.HasValue
                    ? team.Invitations.SingleOrDefault(value => value.Id == invitationId.Value &&
                        value.RecipientAccountId == _currentUser.Id && value.Status == Void.Bot.Message.Enum.ERA.EraTeamInvitationStatus.Accepted &&
                        value.AcceptedMemberId == member.MemberId && value.AcceptedCharacterId == message.CharacterId)
                    : null;
                if (invitationId.HasValue && invitationReceipt == null)
                {
                    return new EraTeamChange<JoinEraTeamResponse>
                    {
                        Result = Error<JoinEraTeamResponse>(HttpStatusCode.Conflict, "era.invitation_consumed", text => text.TeamJoinDenied)
                    };
                }
                if (member.CharacterId != message.CharacterId)
                {
                    return new EraTeamChange<JoinEraTeamResponse>
                    {
                        Result = Error<JoinEraTeamResponse>(HttpStatusCode.Conflict, "era.team_already_joined", text => text.TeamAlreadyJoined)
                    };
                }

                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = new JoinEraTeamResponse
                    {
                        Data = MapTeam(team, now)
                    }
                };
            }

            if (!canJoin || !SameJoinPolicy(team, authorizedTeam))
            {
                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = Error<JoinEraTeamResponse>(HttpStatusCode.Forbidden, "era.team_join_denied", text => text.TeamJoinDenied)
                };
            }
            if (team.Members.Count(item => item.ExpiresAt > now) >= team.MaxMembers)
            {
                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = Error<JoinEraTeamResponse>(HttpStatusCode.Conflict, "era.team_capacity", text => text.TeamCapacity)
                };
            }

            team.Members.RemoveAll(item => item.ExpiresAt <= now);
            team.Members.Add(SetMemberAccount(new EraTeamMember
            {
                MemberId = Guid.NewGuid(),
                JoinedAt = now,
                PlayerId = identity.PlayerId.Value,
                CharacterId = message.CharacterId,
                ExpiresAt = now.AddSeconds(_setting.MemberLifetimeSeconds)
            }));
            var joinedMember = team.Members.Single(item => item.PlayerId == identity.PlayerId.Value);
            if (!ConsumeInvitation(team, invitationId, _currentUser.Id!.Value, joinedMember, now))
            {
                return new EraTeamChange<JoinEraTeamResponse>
                {
                    Result = Error<JoinEraTeamResponse>(HttpStatusCode.Forbidden, "era.invitation_invalid", text => text.TeamJoinDenied)
                };
            }
            // 玩家跨队伍占用检查与队伍提交由同一 Lua 脚本完成。
            return new EraTeamChange<JoinEraTeamResponse>
            {
                Changed = true,
                Team = team,
                Result = new JoinEraTeamResponse
                {
                    Data = MapTeam(team, now)
                }
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> ChangeTeamAsync<T>(
        Guid teamId,
        Func<EraTeam?, EraTeamChange<T>> operation,
        CancellationToken cancellationToken)
        where T : VoidBotResponse, new()
    {
        try
        {
            return await _teams.ChangeAsync(teamId, operation, cancellationToken).ConfigureAwait(false);
        }
        catch (EraTeamWriteException exception)
        {
            if (exception.Status == EraTeamWriteStatus.PlayerAlreadyJoined)
            {
                return Error<T>(HttpStatusCode.Conflict, "era.team_already_joined", text => text.TeamAlreadyJoined);
            }

            if (exception.Status == EraTeamWriteStatus.Expired)
            {
                return Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
            }

            return Error<T>(HttpStatusCode.Conflict, "era.version_conflict", text => text.VersionConflict);
        }
    }

    // 数据库角色校验不在 Redis 原子回调内；提交重试时重新检查队伍及成员有效期。
    private async Task<T> InTeamAsync<T>(
        Guid teamId,
        Guid memberId,
        bool validateCharacter,
        Func<EraTeam, EraTeamMember, DateTimeOffset, T> operation,
        CancellationToken cancellationToken)
        where T : VoidBotResponse, new()
    {
        var identity = await IdentifyAsync<T>(cancellationToken).ConfigureAwait(false);
        if (identity.Error != null)
        {
            return identity.Error;
        }

        var initialTeam = await _teams.GetTeamAsync(teamId, cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        if (initialTeam == null || !IsActive(initialTeam, now))
        {
            return Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
        }

        var initialMember = initialTeam.Members.SingleOrDefault(item =>
            item.PlayerId == identity.PlayerId!.Value && item.ExpiresAt > now);
        if (initialMember == null)
        {
            return Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound);
        }

        if (validateCharacter && initialMember.MemberId != memberId)
        {
            return Error<T>(HttpStatusCode.Conflict, "era.team_membership_changed", text => text.TeamMembershipChanged);
        }

        if (validateCharacter && !await OwnsCharacterAsync(identity.PlayerId!.Value, initialMember.CharacterId, cancellationToken).ConfigureAwait(false))
        {
            return Error<T>(HttpStatusCode.NotFound, "era.character_not_found", text => text.CharacterNotFound);
        }

        // 查询直接使用读到的快照，不写 Redis 或刷新 TTL。
        if (!validateCharacter)
        {
            return operation(initialTeam, initialMember, now);
        }

        return await ChangeTeamAsync(teamId, team =>
        {
            var currentTime = DateTimeOffset.UtcNow;
            if (team == null || !IsActive(team, currentTime))
            {
                return new EraTeamChange<T>
                {
                    Result = Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound)
                };
            }

            var member = team.Members.SingleOrDefault(item => item.PlayerId == identity.PlayerId!.Value && item.ExpiresAt > currentTime);
            if (member == null || member.MemberId != initialMember.MemberId || member.CharacterId != initialMember.CharacterId)
            {
                return new EraTeamChange<T>
                {
                    Result = Error<T>(HttpStatusCode.NotFound, "era.team_not_found", text => text.TeamNotFound)
                };
            }

            SetMemberAccount(member);
            return new EraTeamChange<T>
            {
                Changed = true,
                Team = team,
                Result = operation(team, member, currentTime)
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetCurrentEraTeamResponse> GetCurrentAsync(
        GetCurrentEraTeamRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var identity = await IdentifyAsync<GetCurrentEraTeamResponse>(cancellationToken).ConfigureAwait(false);
            if (identity.Error != null)
            {
                return identity.Error;
            }

            var teamId = await _teams.GetPlayerTeamIdAsync(identity.PlayerId!.Value, cancellationToken).ConfigureAwait(false);
            if (!teamId.HasValue)
            {
                return new GetCurrentEraTeamResponse();
            }

            var team = await _teams.GetTeamAsync(teamId.Value, cancellationToken).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow;
            if (team == null || !IsActive(team, now) ||
                !team.Members.Any(member => member.PlayerId == identity.PlayerId.Value && member.ExpiresAt > now))
            {
                return new GetCurrentEraTeamResponse();
            }

            return new GetCurrentEraTeamResponse
            {
                Data = MapTeam(team, now)
            };
        }
        catch (RedisException)
        {
            return Error<GetCurrentEraTeamResponse>(HttpStatusCode.ServiceUnavailable,
                "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    public async Task<GetEraTeamResponse> GetAsync(GetEraTeamRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await GetCoreAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<GetEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<GetEraTeamResponse> GetCoreAsync(GetEraTeamRequest message, CancellationToken cancellationToken)
    {
        return await InTeamAsync(message.TeamId, Guid.Empty, false, (team, member, now) =>
        {
            return new GetEraTeamResponse
            {
                Data = MapTeam(team, now)
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HeartbeatEraTeamResponse> HeartbeatAsync(HeartbeatEraTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await HeartbeatCoreAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<HeartbeatEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<HeartbeatEraTeamResponse> HeartbeatCoreAsync(HeartbeatEraTeamCommand message, CancellationToken cancellationToken)
    {
        return await InTeamAsync(message.TeamId, message.MemberId, true, (team, member, now) =>
        {
            member.ExpiresAt = now.AddSeconds(_setting.MemberLifetimeSeconds);
            team.ExpiresAt = now.AddSeconds(_setting.TeamLifetimeSeconds);

            return new HeartbeatEraTeamResponse
            {
                Data = MapTeam(team, now)
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LeaveEraTeamResponse> LeaveAsync(LeaveEraTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await LeaveCoreAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<LeaveEraTeamResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<LeaveEraTeamResponse> LeaveCoreAsync(LeaveEraTeamCommand message, CancellationToken cancellationToken)
    {
        var identity = await IdentifyAsync<LeaveEraTeamResponse>(cancellationToken).ConfigureAwait(false);
        if (identity.Error != null)
        {
            return identity.Error;
        }

        return await ChangeTeamAsync(message.TeamId, team =>
        {
            var changed = false;
            if (team != null)
            {
                var currentMember = team.Members.SingleOrDefault(member => member.PlayerId == identity.PlayerId!.Value);
                if (currentMember != null && currentMember.MemberId != message.MemberId)
                {
                    return new EraTeamChange<LeaveEraTeamResponse>
                    {
                        Result = Error<LeaveEraTeamResponse>(HttpStatusCode.Conflict,
                            "era.team_membership_changed", text => text.TeamMembershipChanged)
                    };
                }

                changed = team.Members.RemoveAll(member => member.PlayerId == identity.PlayerId!.Value && member.MemberId == message.MemberId) > 0;
                if (changed && !EraTeamLifecycle.Advance(team, DateTimeOffset.UtcNow, _setting.MigrationTimeoutSeconds))
                {
                    team = null;
                }
            }

            return new EraTeamChange<LeaveEraTeamResponse>
            {
                Changed = changed,
                Team = team,
                Result = new LeaveEraTeamResponse
                {
                    Data = true
                }
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PrepareEraPunchResponse> PreparePunchAsync(PrepareEraPunchCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await PreparePunchCoreAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<PrepareEraPunchResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<PrepareEraPunchResponse> PreparePunchCoreAsync(PrepareEraPunchCommand message, CancellationToken cancellationToken)
    {
        return await InTeamAsync(message.TeamId, message.MemberId, true, (team, member, now) =>
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            if (message.AuthorityEpoch != team.AuthorityEpoch)
            {
                return Error<PrepareEraPunchResponse>(HttpStatusCode.Conflict, "era.authority_changed", text => text.AuthorityChanged);
            }
            member.TicketAuthorityEpoch = team.AuthorityEpoch;
            member.AttemptId = Guid.NewGuid();
            member.TokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            member.LocalAddresses = message.LocalAddresses.Select(address => IPAddress.Parse(address).ToString()).Distinct().ToList();
            member.LocalPort = message.LocalPort;
            member.TicketExpiresAt = now.AddSeconds(_setting.TicketLifetimeSeconds);
            member.Address = null;
            member.Port = 0;
            member.EndpointExpiresAt = default;
            return new PrepareEraPunchResponse
            {
                Data = new EraPunchTicketDto
                {
                    TeamId = team.TeamId,
                    MemberId = member.MemberId,
                    AttemptId = member.AttemptId,
                    AuthorityEpoch = team.AuthorityEpoch,
                    Token = token,
                    ExpiresAt = member.TicketExpiresAt,
                    UdpHost = _setting.PublicHost,
                    UdpPort = _setting.UdpPort
                }
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetEraPunchPeersResponse> GetPeersAsync(GetEraPunchPeersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await GetPeersCoreAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException)
        {
            return Error<GetEraPunchPeersResponse>(HttpStatusCode.ServiceUnavailable, "era.networking_unavailable", text => text.NetworkingUnavailable);
        }
    }

    private async Task<GetEraPunchPeersResponse> GetPeersCoreAsync(GetEraPunchPeersRequest message, CancellationToken cancellationToken)
    {
        return await InTeamAsync(message.TeamId, Guid.Empty, false, (team, member, now) =>
        {
            var peers = team.Members.Where(item => item.MemberId != member.MemberId && item.ExpiresAt > now &&
                item.Address != null && item.EndpointExpiresAt > now && item.TicketExpiresAt > now)
                .Select(item => new EraPunchPeerDto
                {
                    MemberId = item.MemberId,
                    PlayerId = item.PlayerId,
                    AttemptId = item.AttemptId,
                    AuthorityEpoch = team.AuthorityEpoch,
                    IsHost = item.PlayerId == team.HostPlayerId,
                    Address = item.Address!,
                    Port = item.Port,
                    // 有效队伍成员交换网卡候选，允许不同公网出口通过同一 VPN 组网连接。
                    LocalAddresses = [.. item.LocalAddresses],
                    LocalPort = item.LocalPort,
                    ExpiresAt = item.EndpointExpiresAt
                })
                .ToList();
            var state = EraPunchState.NotPrepared;
            if (member.AttemptId != Guid.Empty)
            {
                if (member.TicketExpiresAt <= now)
                {
                    state = EraPunchState.TicketExpired;
                }
                else if (member.Address == null || member.EndpointExpiresAt <= now)
                {
                    state = EraPunchState.WaitingForRegistration;
                }
                else
                {
                    state = EraPunchState.WaitingForPeer;
                    if (peers.Any(peer => member.PlayerId == team.HostPlayerId || peer.IsHost))
                    {
                        state = EraPunchState.ReadyForPunch;
                    }
                }
            }

            return new GetEraPunchPeersResponse
            {
                Data = new EraPunchPeersDto
                {
                    PeersVersion = team.PeersVersion,
                    TeamId = team.TeamId,
                    MemberId = member.MemberId,
                    AttemptId = member.AttemptId,
                    AuthorityEpoch = team.AuthorityEpoch,
                    TicketExpiresAt = member.TicketExpiresAt,
                    State = state,
                    Peers = peers
                }
            };
        }, cancellationToken).ConfigureAwait(false);
    }
}
