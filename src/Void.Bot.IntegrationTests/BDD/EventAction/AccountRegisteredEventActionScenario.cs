using Mediator.Net;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.IntegrationTest.BDD.EventAction;

public class AccountRegisteredEventActionScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private readonly SecurityUtil _securityUtil = new(testBase);

    private Guid _roleId;
    private Guid _userId;

    private async Task GivenRoleExists()
    {
        _roleId = await _securityUtil.AddRoleAsync(
            name: "RegisteredUser",
            code: "REGISTERED_USER");
    }

    private Task GivenEnabledGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(true);
    }

    private Task GivenDisabledGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(false);
    }

    private async Task GivenGrantRoleRuleExists(bool enabled)
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddEventActionRuleCommand, AddEventActionRuleResponse>(
                new AddEventActionRuleCommand
                {
                    Event = BuiltInEventNames.AccountRegistered,
                    Enabled = enabled,
                    Actions =
                    [
                        new ConfiguredEventActionDto
                        {
                            Type = GrantRolesEventActionExecutor.ActionType,
                            Arguments = new Dictionary<string, string[]>
                            {
                                ["RoleIds"] = [_roleId.ToString()]
                            }
                        }
                    ]
                }).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
        });
    }

    private async Task GivenConfiguredRoleNoLongerExists()
    {
        await RunWithUnitOfWork<IRepository<Role>>(async repository =>
        {
            await repository.DeleteAsync(
                role => role.Id == _roleId,
                CancellationToken.None).ConfigureAwait(false);
        });
    }

    private async Task WhenUserRegisters()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = "EventActionUser",
                    Password = StringExtension.GenerateRandomPassword()
                }).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            _userId = response.Data.Id;
        });
    }

    private async Task ThenUserShouldHaveConfiguredRole()
    {
        await Run<IRepository<UserRole>>(async repository =>
        {
            (await repository.AnyAsync(
                userRole => userRole.UserId == _userId && userRole.RoleId == _roleId,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
        });
    }

    private async Task ThenUserShouldNotHaveConfiguredRole()
    {
        await Run<IRepository<UserRole>>(async repository =>
        {
            (await repository.AnyAsync(
                userRole => userRole.UserId == _userId && userRole.RoleId == _roleId,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeFalse();
        });
    }

    private async Task ThenUserAccountShouldRemainRegistered()
    {
        await Run<IRepository<UserAccount>>(async repository =>
        {
            (await repository.AnyAsync(
                account => account.Id == _userId,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
        });
    }

    [Fact]
    public void EnabledRuleGrantsRoleAfterAccountRegistration()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenEnabledGrantRoleRuleExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenUserShouldHaveConfiguredRole())
            .BDDfy();
    }

    [Fact]
    public void DisabledRuleDoesNotGrantRoleAfterAccountRegistration()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenDisabledGrantRoleRuleExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenUserShouldNotHaveConfiguredRole())
            .BDDfy();
    }

    [Fact]
    public void GrantFailureDoesNotRollbackAccountRegistration()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenEnabledGrantRoleRuleExists())
            .And(x => x.GivenConfiguredRoleNoLongerExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenUserAccountShouldRemainRegistered())
            .And(x => x.ThenUserShouldNotHaveConfiguredRole())
            .BDDfy();
    }
}
