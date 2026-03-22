using Autofac;
using Mediator.Net;
using NSubstitute;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Services.Security;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.IntegrationTest.BDD.Security;

public class AssignRoleToUserScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private readonly SecurityUtil _securityUtil = new(testBase);
    private readonly AccountUtil _accountUtil = new(testBase);

    private Guid _userId;
    private Guid _roleId;
    private Guid _permissionId;
    private string _endpoint = "/api/test";

    private async Task GivenUserWithNoRoles()
    {
        _userId = await _accountUtil.AddUserAccountAsync(name: "TestUser");
    }

    private async Task GivenRoleExists()
    {
        _roleId = await _securityUtil.AddRoleAsync(name: "TestRole", code: "TEST_ROLE");
    }

    private async Task GivenPermissionExists()
    {
        _permissionId = await _securityUtil.AddPermissionAsync(name: "TestPermission", code: "TEST_PERM");
    }

    private async Task GivenPermissionAssignedToRole()
    {
        await _securityUtil.AddRolePermissionAsync(_roleId, _permissionId);
    }

    private async Task GivenPermissionHasEndpoint()
    {
        await _securityUtil.AddPermissionEndpointAsync(_permissionId, _endpoint);
    }

    private async Task WhenAssignRoleToUser()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = _userId,
                    RoleIds = [_roleId]
                }).ConfigureAwait(false);

            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);
        });
    }

    private async Task ThenUserShouldHaveRole()
    {
        await Run<IUserRoleService>(async service =>
        {
            var response = await service.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = _userId }, CancellationToken.None);

            response.Data.ShouldNotBeNull();
            response.Data.UserRoles.Count.ShouldBe(1);
            response.Data.UserRoles[0].RoleId.ShouldBe(_roleId);
        });
    }

    private async Task ThenUserShouldHavePermission()
    {
        await Run<IRolePermissionService>(async service =>
        {
            var response = await service.GetPermissionsByRoleIdsAsync(
                new GetPermissionsByRoleIdsRequest { RoleIds = [_roleId] }, CancellationToken.None);

            response.Data.ShouldNotBeNull();
            response.Data.Count.ShouldBe(1);
            response.Data[0].Id.ShouldBe(_permissionId);
        });
    }

    private async Task ThenUserShouldBeAuthorizedForEndpoint()
    {
        var mockEndpointService = Substitute.For<IEndpointService>();
        mockEndpointService.GetEndpointListAsync(Arg.Any<GetEndpointListRequest>())
            .Returns(new GetEndpointListResponse
            {
                Data = [new EndpointDto { Endpoint = _endpoint, Method = "GET" }]
            });

        await Run<ISecurityService>(async service =>
        {
            var result = await service.VerifyPermissionEndpointAsync(
                _userId, _endpoint, "GET", CancellationToken.None);

            result.ShouldBeTrue();
        }, cb => cb.RegisterInstance(mockEndpointService).As<IEndpointService>());
    }

    [Fact]
    public void UserGetsPermissionsAfterRoleAssignment()
    {
        this.Given(x => x.GivenUserWithNoRoles())
            .And(x => x.GivenRoleExists())
            .And(x => x.GivenPermissionExists())
            .And(x => x.GivenPermissionAssignedToRole())
            .And(x => x.GivenPermissionHasEndpoint())
            .When(x => x.WhenAssignRoleToUser())
            .Then(x => x.ThenUserShouldHaveRole())
            .And(x => x.ThenUserShouldHavePermission())
            .And(x => x.ThenUserShouldBeAuthorizedForEndpoint())
            .BDDfy();
    }
}