using Autofac;
using NSubstitute;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Services.Security;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.IntegrationTest.BDD.Security;

public class PermissionAuthorizationScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private static readonly Guid TestUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly SecurityUtil _util = new(testBase);

    private Guid _userId;
    private Guid _superAdminRoleId;
    private Guid _normalRoleId;
    private Guid _permissionId;
    private string _endpoint = "/api/protected";
    private bool _authorizationResult;

    private async Task GivenSuperAdminUser()
    {
        _userId = TestUserId;
        _superAdminRoleId = await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
    }

    private async Task GivenNormalUser()
    {
        _userId = Guid.NewGuid();
        _normalRoleId = await _util.AddRoleAsync(name: "NormalRole", code: "NORMAL");
    }

    private async Task GivenSuperAdminRoleAssigned()
    {
        await _util.AddUserRoleAsync(_userId, _superAdminRoleId);
    }

    private async Task GivenNormalRoleAssigned()
    {
        await _util.AddUserRoleAsync(_userId, _normalRoleId);
    }

    private async Task GivenPermissionExists()
    {
        _permissionId = await _util.AddPermissionAsync(name: "ProtectedPermission", code: "PROTECTED");
    }

    private async Task GivenPermissionAssignedToNormalRole()
    {
        await _util.AddRolePermissionAsync(_normalRoleId, _permissionId);
    }

    private async Task GivenPermissionHasEndpoint()
    {
        await _util.AddPermissionEndpointAsync(_permissionId, _endpoint);
    }

    private async Task WhenCheckSuperAdminStatus()
    {
        await Run<IUserRoleService>(async service =>
        {
            _authorizationResult = await service.CheckUserIsSuperAdminAsync(_userId, CancellationToken.None);
        });
    }

    private async Task WhenCheckEndpointAuthorization()
    {
        var mockEndpointService = Substitute.For<IEndpointService>();
        mockEndpointService.GetEndpointListAsync(Arg.Any<GetEndpointListRequest>())
            .Returns(new GetEndpointListResponse
            {
                Data = [new EndpointDto { Endpoint = _endpoint, Method = "GET" }]
            });

        await Run<ISecurityService>(async service =>
        {
            _authorizationResult = await service.VerifyPermissionEndpointAsync(
                _userId, _endpoint, "GET", CancellationToken.None);
        }, cb => cb.RegisterInstance(mockEndpointService).As<IEndpointService>());
    }

    private void ThenUserShouldBeSuperAdmin()
    {
        _authorizationResult.ShouldBeTrue();
    }

    private void ThenUserShouldNotBeSuperAdmin()
    {
        _authorizationResult.ShouldBeFalse();
    }

    private void ThenUserShouldBeAuthorized()
    {
        _authorizationResult.ShouldBeTrue();
    }

    private void ThenUserShouldNotBeAuthorized()
    {
        _authorizationResult.ShouldBeFalse();
    }

    [Fact]
    public void SuperAdminUserCanBeIdentified()
    {
        this.Given(x => x.GivenSuperAdminUser())
            .And(x => x.GivenSuperAdminRoleAssigned())
            .When(x => x.WhenCheckSuperAdminStatus())
            .Then(x => x.ThenUserShouldBeSuperAdmin())
            .BDDfy();
    }

    [Fact]
    public void NormalUserIsNotIdentifiedAsSuperAdmin()
    {
        this.Given(x => x.GivenNormalUser())
            .And(x => x.GivenNormalRoleAssigned())
            .When(x => x.WhenCheckSuperAdminStatus())
            .Then(x => x.ThenUserShouldNotBeSuperAdmin())
            .BDDfy();
    }

    [Fact]
    public void UserWithPermissionCanAccessEndpoint()
    {
        this.Given(x => x.GivenNormalUser())
            .And(x => x.GivenNormalRoleAssigned())
            .And(x => x.GivenPermissionExists())
            .And(x => x.GivenPermissionAssignedToNormalRole())
            .And(x => x.GivenPermissionHasEndpoint())
            .When(x => x.WhenCheckEndpointAuthorization())
            .Then(x => x.ThenUserShouldBeAuthorized())
            .BDDfy();
    }

    [Fact]
    public void UserWithoutPermissionCannotAccessEndpoint()
    {
        this.Given(x => x.GivenNormalUser())
            .And(x => x.GivenNormalRoleAssigned())
            .And(x => x.GivenPermissionExists())
            .And(x => x.GivenPermissionHasEndpoint())
            .When(x => x.WhenCheckEndpointAuthorization())
            .Then(x => x.ThenUserShouldNotBeAuthorized())
            .BDDfy();
    }
}