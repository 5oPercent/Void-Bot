using System.Net;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.World;

public interface IEraModularActivityService : IScope
{
    Task<Guid> ApproveAsync(Guid worldId, ApproveEraModularActivity request, CancellationToken cancellationToken);
    Task<JsonElement> ClaimAsync(Guid worldId, ClaimEraModularActivity request, CancellationToken cancellationToken);
    Task<GrantEraGroundRewardCommand> AuthorizeEquipmentAsync(GrantApprovedEraGroundRewardCommand request, CancellationToken cancellationToken);
}

public sealed class EraModularActivityService(
    IEraWorldDataProvider worlds, IEraTeamDataProvider teams, IEraPlayerContext players, IEraActivityApprovalStore approvals,
    ICurrentUser currentUser, IUserRoleService roles, ILocalizationService localization, TimeProvider clock) : IEraModularActivityService
{
    private sealed record Approval(Guid AccountId, Guid PlayerId, Guid CharacterId, JsonElement Selection);
    private sealed record EquipmentSelection(Guid RequestId, Guid CharacterId, Guid PlacementMemberId,
        List<EraEquipmentRewardEntry> Entries, bool IsPublic);
    private static readonly JsonSerializerOptions AdminJson = new(JsonSerializerDefaults.Web);

    public async Task<Guid> ApproveAsync(Guid worldId, ApproveEraModularActivity request, CancellationToken cancellationToken)
    {
        Guid accountId = currentUser.Id.GetValueOrDefault();
        if (accountId == Guid.Empty || !await roles.CheckUserIsSuperAdminAsync(accountId, cancellationToken).ConfigureAwait(false))
            throw SuperAdminRequired();
        if (request.Selection.ValueKind != JsonValueKind.Object ||
            Encoding.UTF8.GetByteCount(request.Selection.GetRawText()) > (request.IsAdminWorldAction ? 1048576 : 65536) ||
            request.IsAdminWorldAction && string.IsNullOrWhiteSpace(ActionKind(request.Selection)))
            throw InvalidOperation();

        var (snapshot, team) = await RequireContextAsync(worldId, request.MemberId, request.AuthorityEpoch, false, cancellationToken).ConfigureAwait(false);
        var member = team.Members.Single(member => member.MemberId == request.MemberId);
        Guid id = Guid.NewGuid();
        var key = Key(snapshot.World, request.MemberId, id, request.IsAdminWorldAction);
        var approval = new Approval(accountId, member.PlayerId, member.CharacterId, request.Selection);
        try
        {
            if (!await approvals.StoreAsync(key, JsonSerializer.Serialize(approval), cancellationToken).ConfigureAwait(false))
                throw ApprovalUnavailable();
        }
        catch (RedisException) { throw NetworkingUnavailable(); }
        return id;
    }

    public async Task<JsonElement> ClaimAsync(Guid worldId, ClaimEraModularActivity request, CancellationToken cancellationToken)
    {
        if (request.ApprovalId == Guid.Empty || request.InitiatorMemberId == Guid.Empty) throw InvalidOperation();
        var (snapshot, team) = await RequireContextAsync(worldId, request.MemberId, request.AuthorityEpoch, true, cancellationToken).ConfigureAwait(false);
        var key = Key(snapshot.World, request.InitiatorMemberId, request.ApprovalId, request.IsAdminWorldAction);
        try
        {
            var json = await approvals.ReadAsync(key, cancellationToken).ConfigureAwait(false);
            var approval = json == null ? null : JsonSerializer.Deserialize<Approval>(json);
            if (approval == null || !IsContinuing(snapshot, team, request.InitiatorMemberId, approval.PlayerId, approval.CharacterId))
                throw ApprovalUnavailable();
            if (!await roles.CheckUserIsSuperAdminAsync(approval.AccountId, cancellationToken).ConfigureAwait(false))
                throw SuperAdminRequired();
            // 装备还需主机计算落点并提交；正式奖励以固定请求 ID 防重。
            bool equipment = request.IsAdminWorldAction && ActionKind(approval.Selection) == "equipment";
            if (!equipment && !await approvals.ConsumeAsync(key, cancellationToken).ConfigureAwait(false))
                throw ApprovalUnavailable();
            return approval.Selection;
        }
        catch (RedisException) { throw NetworkingUnavailable(); }
    }

    public async Task<GrantEraGroundRewardCommand> AuthorizeEquipmentAsync(GrantApprovedEraGroundRewardCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ApprovalId == Guid.Empty || request.InitiatorMemberId == Guid.Empty || request.Placement == null)
            throw InvalidOperation();
        var (snapshot, team) = await RequireContextAsync(request.WorldInstanceId, request.MemberId, request.AuthorityEpoch,
            true, cancellationToken).ConfigureAwait(false);
        var key = Key(snapshot.World, request.InitiatorMemberId, request.ApprovalId, true);
        try
        {
            var json = await approvals.ReadAsync(key, cancellationToken).ConfigureAwait(false);
            var approval = json == null ? null : JsonSerializer.Deserialize<Approval>(json);
            if (approval == null || ActionKind(approval.Selection) != "equipment" ||
                !IsContinuing(snapshot, team, request.InitiatorMemberId, approval.PlayerId, approval.CharacterId))
                throw ApprovalUnavailable();
            if (!await roles.CheckUserIsSuperAdminAsync(approval.AccountId, cancellationToken).ConfigureAwait(false))
                throw SuperAdminRequired();
            var payload = ActionProperty(approval.Selection, "payload", "Payload");
            var equipment = payload.ValueKind == JsonValueKind.Object ? payload.Deserialize<EquipmentSelection>(AdminJson) : null;
            var anchor = equipment == null ? null : team.Members.SingleOrDefault(member => member.MemberId == equipment.PlacementMemberId);
            if (equipment == null || equipment.RequestId == Guid.Empty || anchor == null ||
                !IsContinuing(snapshot, team, anchor.MemberId, anchor.PlayerId, anchor.CharacterId) ||
                (equipment.IsPublic ? equipment.CharacterId != approval.CharacterId :
                    anchor.MemberId != request.InitiatorMemberId && anchor.CharacterId != equipment.CharacterId))
                throw InvalidOperation();
            return new GrantEraGroundRewardCommand
            {
                WorldInstanceId = request.WorldInstanceId, MemberId = request.MemberId, AuthorityEpoch = request.AuthorityEpoch,
                RequestId = equipment.RequestId, CharacterId = equipment.CharacterId, IsPublic = equipment.IsPublic,
                Entries = equipment.Entries, Placement = request.Placement
            };
        }
        catch (RedisException) { throw NetworkingUnavailable(); }
        catch (JsonException) { throw InvalidOperation(); }
    }

    private static JsonElement ActionProperty(JsonElement selection, string camelCase, string pascalCase) =>
        selection.ValueKind == JsonValueKind.Object &&
        (selection.TryGetProperty(camelCase, out var property) || selection.TryGetProperty(pascalCase, out property))
            ? property : default;

    private static string? ActionKind(JsonElement selection)
    {
        var kind = ActionProperty(selection, "kind", "Kind");
        return kind.ValueKind == JsonValueKind.String ? kind.GetString() : null;
    }

    private async Task<(EraWorldReadSnapshot Snapshot, EraTeam Team)> RequireContextAsync(Guid worldId, Guid memberId,
        long epoch, bool requireHost, CancellationToken cancellationToken)
    {
        if (worldId == Guid.Empty || memberId == Guid.Empty || epoch < 1) throw InvalidOperation();
        var player = await players.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);
        var snapshot = await worlds.ReadSnapshotAsync(worldId, player.Id, cancellationToken).ConfigureAwait(false);
        if (snapshot == null || snapshot.World.State != EraWorldState.Ready || snapshot.World.AuthorityEpoch != epoch)
            throw AuthorityChanged();
        EraTeam? team;
        try { team = await teams.GetTeamAsync(snapshot.World.TeamId, cancellationToken).ConfigureAwait(false); }
        catch (RedisException) { throw NetworkingUnavailable(); }
        var now = clock.GetUtcNow();
        if (team == null || team.ExpiresAt <= now || team.IsMigrating || team.AuthorityEpoch != epoch ||
            team.HostPlayerId != snapshot.World.HostPlayerId ||
            !team.Members.Any(member => member.PlayerId == team.HostPlayerId && member.ExpiresAt > now))
            throw AuthorityChanged();
        if (requireHost && player.Id != team.HostPlayerId)
            throw new EraWorldAccessException(HttpStatusCode.Forbidden, "era.world_host_required",
                localization.GetValue<EraWorldContextTemplate>(text => text.HostRequired));
        var actor = team.Members.SingleOrDefault(member => member.MemberId == memberId && member.PlayerId == player.Id);
        if (actor == null || !IsContinuing(snapshot, team, memberId, player.Id, actor.CharacterId))
            throw new EraWorldAccessException(HttpStatusCode.Forbidden, "era.world_member_unavailable",
                localization.GetValue<EraWorldContextTemplate>(text => text.MemberUnavailable));
        return (snapshot, team);
    }

    private bool IsContinuing(EraWorldReadSnapshot snapshot, EraTeam team, Guid memberId, Guid playerId, Guid characterId)
    {
        var now = clock.GetUtcNow();
        return team.Members.Any(member => member.MemberId == memberId && member.PlayerId == playerId &&
                   member.CharacterId == characterId && member.ExpiresAt > now) &&
               snapshot.Members.Any(member => member.MemberId == memberId && member.PlayerId == playerId &&
                   member.CharacterId == characterId && member.IsContinuing && member.ActivePlayerId == playerId &&
                   member.ActiveCharacterId == characterId);
    }

    private static string Key(EraWorld world, Guid initiator, Guid approval, bool adminWorldAction) =>
        $"{(adminWorldAction ? "admin-action:" : string.Empty)}{world.TeamId:N}:{world.Id:N}:{world.AuthorityEpoch}:{initiator:N}:{approval:N}";

    private EraWorldAccessException SuperAdminRequired() => new(HttpStatusCode.Forbidden, "era.super_admin_required",
        localization.GetValue<EraWorldContextTemplate>(text => text.SuperAdminRequired));
    private EraWorldAccessException ApprovalUnavailable() => new(HttpStatusCode.Conflict, "era.modular_activity_approval_unavailable",
        localization.GetValue<EraWorldContextTemplate>(text => text.ModularActivityApprovalUnavailable));
    private EraWorldAccessException AuthorityChanged() => new(HttpStatusCode.Conflict, "era.world_authority_changed",
        localization.GetValue<EraWorldContextTemplate>(text => text.AuthorityChanged));
    private EraWorldAccessException InvalidOperation() => new(HttpStatusCode.BadRequest, "era.world_invalid_operation",
        localization.GetValue<EraWorldContextTemplate>(text => text.InvalidOperation));
    private EraWorldAccessException NetworkingUnavailable() => new(HttpStatusCode.ServiceUnavailable, "era.world_networking_unavailable",
        localization.GetValue<EraWorldContextTemplate>(text => text.NetworkingUnavailable));
}
