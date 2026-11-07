using System.Collections.Concurrent;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed class EraModularActivityFixture
{
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly IUserRoleService _roles = Substitute.For<IUserRoleService>();
    private readonly IEraPlayerContext _players = Substitute.For<IEraPlayerContext>();
    private readonly IEraActivityApprovalStore _approvals = Substitute.For<IEraActivityApprovalStore>();
    private readonly EraTeamMember _guest = Member();
    private readonly EraTeamMember _host = Member();
    private readonly Guid _guestAccount = Guid.NewGuid();
    private readonly Guid _hostAccount = Guid.NewGuid();
    private readonly Guid _worldId = Guid.NewGuid();
    private readonly EraTeam _team;
    private readonly EraModularActivityService _service;
    private const long Epoch = 3;

    public EraModularActivityFixture()
    {
        _team = new EraTeam { TeamId = Guid.NewGuid(), HostPlayerId = _host.PlayerId,
            AuthorityEpoch = Epoch, ExpiresAt = _host.ExpiresAt, Members = [_guest, _host] };
        var world = new EraWorld { Id = _worldId, TeamId = _team.TeamId, HostPlayerId = _host.PlayerId,
            AuthorityEpoch = Epoch, State = EraWorldState.Ready };
        var worlds = Substitute.For<IEraWorldDataProvider>();
        var snapshot = new EraWorldReadSnapshot(world, _team.Members.Select(member => new EraWorldMember
        {
            PlayerId = member.PlayerId, CharacterId = member.CharacterId, MemberId = member.MemberId,
            IsContinuing = true, ActivePlayerId = member.PlayerId, ActiveCharacterId = member.CharacterId
        }).ToArray());
        worlds.ReadSnapshotAsync(_worldId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(snapshot);
        var teams = Substitute.For<IEraTeamDataProvider>();
        teams.GetTeamAsync(_team.TeamId, Arg.Any<CancellationToken>()).Returns(_team);

        var stored = new ConcurrentDictionary<string, string>();
        _approvals.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(stored.TryAdd(call.ArgAt<string>(0), call.ArgAt<string>(1))));
        _approvals.ReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(stored.GetValueOrDefault(call.ArgAt<string>(0))));
        _approvals.ConsumeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(stored.TryRemove(call.ArgAt<string>(0), out _)));
        _roles.CheckUserIsSuperAdminAsync(_guestAccount, Arg.Any<CancellationToken>()).Returns(true);
        _service = new EraModularActivityService(worlds, teams, _players, _approvals, _user, _roles,
            Substitute.For<ILocalizationService>(), TimeProvider.System);
    }

    [Fact]
    public async Task Guest_admin_can_submit_and_only_current_host_can_claim_once()
    {
        UseMember(_guest, _guestAccount);
        var id = await ApproveAsync(_guest.MemberId);
        var request = new ClaimEraModularActivity(_host.MemberId, Epoch, _guest.MemberId, id);
        await Should.ThrowAsync<EraWorldAccessException>(() => ClaimAsync(request with { MemberId = _guest.MemberId }));

        UseMember(_host, _hostAccount);
        await Should.ThrowAsync<EraWorldAccessException>(() => ClaimAsync(request with { InitiatorMemberId = _host.MemberId }));
        var selection = await ClaimAsync(request);
        selection.GetProperty("suite").GetString().ShouldBe("graybox");
        await Should.ThrowAsync<EraWorldAccessException>(() => ClaimAsync(request));
    }

    [Fact]
    public async Task Host_admin_can_approve_and_claim_own_request()
    {
        UseMember(_host, _hostAccount);
        _roles.CheckUserIsSuperAdminAsync(_hostAccount, Arg.Any<CancellationToken>()).Returns(true);
        var id = await ApproveAsync(_host.MemberId);
        var selection = await ClaimAsync(new ClaimEraModularActivity(_host.MemberId, Epoch, _host.MemberId, id));
        selection.GetProperty("suite").GetString().ShouldBe("graybox");
    }

    [Fact]
    public async Task Ordinary_host_cannot_submit_and_revoked_guest_cannot_open_preparation()
    {
        UseMember(_host, _hostAccount);
        await Should.ThrowAsync<EraWorldAccessException>(() => ApproveAsync(_host.MemberId));
        UseMember(_guest, _guestAccount);
        var id = await ApproveAsync(_guest.MemberId);
        _roles.CheckUserIsSuperAdminAsync(_guestAccount, Arg.Any<CancellationToken>()).Returns(false);

        UseMember(_host, _hostAccount);
        await Should.ThrowAsync<EraWorldAccessException>(() =>
            ClaimAsync(new ClaimEraModularActivity(_host.MemberId, Epoch, _guest.MemberId, id)));
        await _approvals.DidNotReceive().ConsumeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Approval_from_old_authority_epoch_cannot_be_claimed()
    {
        UseMember(_guest, _guestAccount);
        var id = await ApproveAsync(_guest.MemberId);
        _team.AuthorityEpoch++;

        UseMember(_host, _hostAccount);
        await Should.ThrowAsync<EraWorldAccessException>(() =>
            ClaimAsync(new ClaimEraModularActivity(_host.MemberId, Epoch, _guest.MemberId, id)));
        await _approvals.DidNotReceive().ReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void UseMember(EraTeamMember member, Guid account)
    {
        _user.Id.Returns(account);
        _players.RequireCurrentAsync(false, Arg.Any<CancellationToken>()).Returns(new EraPlayer { Id = member.PlayerId });
    }

    private Task<Guid> ApproveAsync(Guid member) => _service.ApproveAsync(_worldId,
        new ApproveEraModularActivity(member, Epoch, JsonSerializer.SerializeToElement(new { suite = "graybox" })), CancellationToken.None);

    private Task<JsonElement> ClaimAsync(ClaimEraModularActivity request) =>
        _service.ClaimAsync(_worldId, request, CancellationToken.None);

    private static EraTeamMember Member() => new()
    {
        MemberId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), CharacterId = Guid.NewGuid(),
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
    };
}
