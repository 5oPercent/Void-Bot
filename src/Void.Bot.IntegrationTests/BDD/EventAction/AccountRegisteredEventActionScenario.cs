using System.Globalization;
using System.Net;
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
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.IntegrationTest.BDD.EventAction;

public class AccountRegisteredEventActionScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private const string RuleName = "Grant registered user role";
    private const int DurationHours = 2;

    private readonly SecurityUtil _securityUtil = new(testBase);

    private Guid _roleId;
    private Guid _userId;
    private string _listedRuleName = string.Empty;
    private HttpStatusCode _ruleResponseCode;
    private DateTimeOffset _registrationStartedAt;
    private DateTimeOffset _registrationCompletedAt;

    private async Task GivenRoleExists()
    {
        _roleId = await _securityUtil.AddRoleAsync(
            name: "RegisteredUser",
            code: "REGISTERED_USER");
    }

    private Task GivenEnabledGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(true, null, null);
    }

    private Task GivenEnabledGrantRoleRuleWithDurationExists()
    {
        return GivenGrantRoleRuleExists(true, DurationHours, "Hour");
    }

    private Task GivenDisabledGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(false, null, null);
    }

    private async Task GivenGrantRoleRuleExists(bool enabled, int? duration, string? durationUnit)
    {
        var arguments = new Dictionary<string, string[]>
        {
            ["RoleIds"] = [_roleId.ToString()]
        };
        if (duration.HasValue)
        {
            arguments["Duration"] =
                [duration.Value.ToString(CultureInfo.InvariantCulture)];
            arguments["DurationUnit"] = [durationUnit!];
        }

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddEventActionRuleCommand, AddEventActionRuleResponse>(
                new AddEventActionRuleCommand
                {
                    Name = RuleName,
                    Event = BuiltInEventNames.AccountRegistered,
                    Enabled = enabled,
                    Actions =
                    [
                        new ConfiguredEventActionDto
                        {
                            Type = GrantRolesEventActionExecutor.ActionType,
                            Arguments = arguments
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
        _registrationStartedAt = DateTimeOffset.UtcNow;
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
        _registrationCompletedAt = DateTimeOffset.UtcNow;
    }

    private async Task WhenRulesAreQueried()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<
                GetEventActionRuleListRequest,
                GetEventActionRuleListResponse>(new GetEventActionRuleListRequest
            {
                Event = BuiltInEventNames.AccountRegistered
            }).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            _listedRuleName = response.Data.Single().Name;
        });
    }

    private async Task WhenGrantRoleRuleWithInvalidDurationIsAdded()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddEventActionRuleCommand, AddEventActionRuleResponse>(
                new AddEventActionRuleCommand
                {
                    Name = RuleName,
                    Event = BuiltInEventNames.AccountRegistered,
                    Actions =
                    [
                        new ConfiguredEventActionDto
                        {
                            Type = GrantRolesEventActionExecutor.ActionType,
                            Arguments = new Dictionary<string, string[]>
                            {
                                ["RoleIds"] = [_roleId.ToString()],
                                ["Duration"] = ["0"],
                                ["DurationUnit"] = ["Second"]
                            }
                        }
                    ]
                }).ConfigureAwait(false);

            _ruleResponseCode = response.Code;
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

    private Task ThenRuleShouldHaveCustomName()
    {
        _listedRuleName.ShouldBe(RuleName);
        return Task.CompletedTask;
    }

    private Task ThenRuleShouldBeRejected()
    {
        _ruleResponseCode.ShouldBe(HttpStatusCode.BadRequest);
        return Task.CompletedTask;
    }

    private async Task ThenRoleShouldExpireAfterConfiguredDuration()
    {
        await Run<IRepository<UserRole>>(async repository =>
        {
            var userRole = await repository.SingleOrDefaultAsync(
                item => item.UserId == _userId && item.RoleId == _roleId,
                CancellationToken.None).ConfigureAwait(false);

            userRole.ShouldNotBeNull();
            userRole.Deadline.HasValue.ShouldBeTrue();
            (userRole.Deadline.Value >= _registrationStartedAt.AddHours(DurationHours)).ShouldBeTrue();
            (userRole.Deadline.Value <= _registrationCompletedAt.AddHours(DurationHours)).ShouldBeTrue();
        });
    }

    [Fact]
    public void RuleCanHaveCustomName()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenEnabledGrantRoleRuleExists())
            .When(x => x.WhenRulesAreQueried())
            .Then(x => x.ThenRuleShouldHaveCustomName())
            .BDDfy();
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
    public void ConfiguredDurationSetsRoleDeadline()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenEnabledGrantRoleRuleWithDurationExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenRoleShouldExpireAfterConfiguredDuration())
            .BDDfy();
    }

    [Fact]
    public void InvalidDurationIsRejected()
    {
        this.Given(x => x.GivenRoleExists())
            .When(x => x.WhenGrantRoleRuleWithInvalidDurationIsAdded())
            .Then(x => x.ThenRuleShouldBeRejected())
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
