using System.Globalization;
using System.Net;
using Mediator.Net;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.EventAction;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.EventAction;
using Void.Bot.Message.Constants.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Events.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.IntegrationTest.BDD.EventAction;

public class AccountRegisteredEventActionScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private const string RuleName = "Grant registered user role";
    private const int DurationHours = 2;

    private readonly SecurityUtil _securityUtil = new(testBase);

    private Guid _roleId;
    private Guid _secondRoleId;
    private Guid _userId;
    private EventActionDefinitionData _definitions = new();
    private string _listedRuleName = string.Empty;
    private HttpStatusCode _ruleResponseCode;

    private async Task GivenRoleExists()
    {
        _roleId = await _securityUtil.AddRoleAsync(
            name: "RegisteredUser",
            code: "REGISTERED_USER");
    }

    private async Task GivenTwoRolesExist()
    {
        await GivenRoleExists();
        _secondRoleId = await _securityUtil.AddRoleAsync(
            name: "SecondRegisteredUser",
            code: "SECOND_REGISTERED_USER");
    }

    private Task GivenEnabledGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(true, null, null);
    }

    private Task GivenEnabledPasswordForceResetGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(
            BuiltInEventNames.AccountPasswordForceReset, true, null, null);
    }

    private Task GivenEnabledThirdPartyForceUnlinkedGrantRoleRuleExists()
    {
        return GivenGrantRoleRuleExists(
            BuiltInEventNames.AccountThirdPartyForceUnlinked,
            true,
            null,
            null,
            [
                new EventActionConditionDto
                {
                    Field = "Platform",
                    Operator = EventActionConditionOperators.Equal,
                    Values = [AppPlatformEnum.Discord.ToString()]
                }
            ]);
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
        await GivenGrantRoleRuleExists(
            BuiltInEventNames.AccountRegistered,
            enabled,
            duration,
            durationUnit);
    }

    private async Task GivenGrantRoleRuleExists(
        string eventName,
        bool enabled,
        int? duration,
        string? durationUnit,
        List<EventActionConditionDto>? conditions = null)
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
                    Event = eventName,
                    Enabled = enabled,
                    Conditions = conditions ?? [],
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

    private async Task GivenUnmatchedConditionRuleExists()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddEventActionRuleCommand, AddEventActionRuleResponse>(
                new AddEventActionRuleCommand
                {
                    Name = RuleName,
                    Event = BuiltInEventNames.AccountRegistered,
                    Conditions =
                    [
                        new EventActionConditionDto
                        {
                            Field = "UserId",
                            Operator = EventActionConditionOperators.Equal,
                            Values = [Guid.NewGuid().ToString()]
                        }
                    ],
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

    private async Task GivenContinueAfterFailureRuleExists()
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
                            FailurePolicy = EventActionFailurePolicies.Continue,
                            Arguments = new Dictionary<string, string[]>
                            {
                                ["RoleIds"] = [_roleId.ToString()]
                            }
                        },
                        new ConfiguredEventActionDto
                        {
                            Type = GrantRolesEventActionExecutor.ActionType,
                            Arguments = new Dictionary<string, string[]>
                            {
                                ["RoleIds"] = [_secondRoleId.ToString()]
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

    private async Task GivenRegisteredUserExists()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = "ManagedEventUser",
                    Password = StringExtension.GenerateRandomPassword()
                }).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            _userId = response.Data.Id;
        });
    }

    private Task GivenDiscordBindingExists()
    {
        return RunWithUnitOfWork<IRepository<UserThirdParty>>(repository =>
            repository.InsertAsync(new UserThirdParty
            {
                UserId = _userId,
                Provider = AppPlatformEnum.Discord.ToString(),
                ThirdPartyUserId = "event-system-discord-user"
            }, CancellationToken.None));
    }

    private async Task WhenUserPasswordIsForceReset()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<
                ForceResetUserAccountPasswordCommand,
                ForceResetUserAccountPasswordResponse>(new ForceResetUserAccountPasswordCommand
            {
                UserId = _userId,
                NewPassword = "ResetPassword1",
                NewPasswordConfirm = "ResetPassword1"
            }).ConfigureAwait(false);

            response.Code.ShouldBe(HttpStatusCode.OK);
        });
    }

    private async Task WhenDiscordBindingIsForceUnlinked()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<
                ForceUnlinkUserAccountThirdPartyCommand,
                ForceUnlinkUserAccountThirdPartyResponse>(new ForceUnlinkUserAccountThirdPartyCommand
            {
                UserId = _userId,
                Platform = AppPlatformEnum.Discord
            }).ConfigureAwait(false);

            response.Code.ShouldBe(HttpStatusCode.OK);
        });
    }

    private Task GivenMissingUserId()
    {
        _userId = Guid.NewGuid();
        return Task.CompletedTask;
    }

    private async Task WhenMissingUserPasswordIsForceReset()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<
                ForceResetUserAccountPasswordCommand,
                ForceResetUserAccountPasswordResponse>(new ForceResetUserAccountPasswordCommand
            {
                UserId = _userId,
                NewPassword = "ResetPassword1",
                NewPasswordConfirm = "ResetPassword1"
            }).ConfigureAwait(false);

            response.Code.ShouldBe(HttpStatusCode.NotFound);
        });
    }

    private async Task WhenMissingDiscordBindingIsForceUnlinked()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<
                ForceUnlinkUserAccountThirdPartyCommand,
                ForceUnlinkUserAccountThirdPartyResponse>(new ForceUnlinkUserAccountThirdPartyCommand
            {
                UserId = _userId,
                Platform = AppPlatformEnum.Discord
            }).ConfigureAwait(false);

            response.Code.ShouldBe(HttpStatusCode.NotFound);
        });
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

    private async Task WhenEventActionDefinitionsAreQueried()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<
                GetEventActionDefinitionRequest,
                GetEventActionDefinitionResponse>(new GetEventActionDefinitionRequest()).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            _definitions = response.Data;
        });
    }

    private async Task WhenGrantRoleRuleWithUnsupportedVersionIsAdded()
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
                            Version = 2,
                            Arguments = new Dictionary<string, string[]>
                            {
                                ["RoleIds"] = [_roleId.ToString()]
                            }
                        }
                    ]
                }).ConfigureAwait(false);

            _ruleResponseCode = response.Code;
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

    private async Task ThenUserShouldHaveSecondConfiguredRole()
    {
        await Run<IRepository<UserRole>>(async repository =>
        {
            (await repository.AnyAsync(
                userRole => userRole.UserId == _userId && userRole.RoleId == _secondRoleId,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
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

    private Task ThenAccountRegisteredEventShouldBeDefined()
    {
        var eventDefinition = _definitions.Events.Single(x =>
            x.Name == BuiltInEventNames.AccountRegistered);
        eventDefinition.Fields.ShouldContain(x =>
            x.Name == "UserId" && x.ValueType == "Guid");

        var passwordForceResetDefinition = _definitions.Events.Single(x =>
            x.Name == BuiltInEventNames.AccountPasswordForceReset);
        passwordForceResetDefinition.Fields.ShouldContain(x =>
            x.Name == "UserId" && x.ValueType == "Guid");

        var thirdPartyForceUnlinkedDefinition = _definitions.Events.Single(x =>
            x.Name == BuiltInEventNames.AccountThirdPartyForceUnlinked);
        thirdPartyForceUnlinkedDefinition.Fields.ShouldContain(x =>
            x.Name == "UserId" && x.ValueType == "Guid");
        thirdPartyForceUnlinkedDefinition.Fields.ShouldContain(x =>
            x.Name == "Platform" &&
            x.ValueType == "String" &&
            x.Options.Any(option => option.Value == AppPlatformEnum.Discord.ToString()));

        var actionDefinition = _definitions.Actions.Single(x =>
            x.Type == GrantRolesEventActionExecutor.ActionType);
        actionDefinition.Version.ShouldBe(1);
        actionDefinition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountPasswordForceReset);
        actionDefinition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountThirdPartyForceUnlinked);
        var durationDefinition = actionDefinition.Arguments.Single(x => x.Name == "Duration");
        durationDefinition.ValueType.ShouldBe("Integer");
        durationDefinition.Minimum.ShouldBe(1);
        durationDefinition.RequiredWith.ShouldContain("DurationUnit");
        durationDefinition.MaximumByOption["Hour"].ShouldBe(876000L);

        var sendEmailDefinition = _definitions.Actions.Single(x =>
            x.Type == SendEmailEventActionExecutor.ActionType);
        sendEmailDefinition.Version.ShouldBe(1);
        sendEmailDefinition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountRegistered);
        sendEmailDefinition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountPasswordForceReset);
        sendEmailDefinition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountThirdPartyForceUnlinked);
        sendEmailDefinition.Arguments.ShouldContain(x => x.Name == "Subject" && x.Required);
        sendEmailDefinition.Arguments.ShouldContain(x => x.Name == "Content" && x.Required);

        _definitions.FailurePolicies.Select(x => x.Value)
            .ShouldContain(EventActionFailurePolicies.Continue);
        return Task.CompletedTask;
    }

    private async Task ThenSuccessfulExecutionShouldBeRecorded()
    {
        await Run<IRepository<EventActionExecution>>(async repository =>
        {
            (await repository.AnyAsync(
                execution => execution.RuleName == RuleName &&
                             execution.ActionType == GrantRolesEventActionExecutor.ActionType &&
                             execution.ActionVersion == 1 &&
                             execution.Status == EventActionExecutionStatuses.Succeeded,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
        });
    }

    private async Task ThenExecutionShouldNotBeRecorded()
    {
        await Run<IRepository<EventActionExecution>>(async repository =>
        {
            (await repository.AnyAsync(
                execution => execution.RuleName == RuleName,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeFalse();
        });
    }

    private async Task ThenFailedAndSuccessfulExecutionsShouldBeRecorded()
    {
        await Run<IRepository<EventActionExecution>>(async repository =>
        {
            (await repository.AnyAsync(
                execution => execution.RuleName == RuleName &&
                             execution.ActionIndex == 0 &&
                             execution.Status == EventActionExecutionStatuses.Failed,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
            (await repository.AnyAsync(
                execution => execution.RuleName == RuleName &&
                             execution.ActionIndex == 1 &&
                             execution.Status == EventActionExecutionStatuses.Succeeded,
                CancellationToken.None).ConfigureAwait(false)).ShouldBeTrue();
        });
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
            var expectedDeadline = DateTimeOffset.UtcNow.AddHours(DurationHours);
            userRole.Deadline.Value.ShouldBe(
                expectedDeadline,
                tolerance: TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public void EventDefinitionsAreDiscoveredFromProviders()
    {
        this.When(x => x.WhenEventActionDefinitionsAreQueried())
            .Then(x => x.ThenAccountRegisteredEventShouldBeDefined())
            .BDDfy();
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
            .And(x => x.ThenSuccessfulExecutionShouldBeRecorded())
            .BDDfy();
    }

    [Fact]
    public void EnabledRuleRunsAfterPasswordIsForceReset()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenRegisteredUserExists())
            .And(x => x.GivenEnabledPasswordForceResetGrantRoleRuleExists())
            .When(x => x.WhenUserPasswordIsForceReset())
            .Then(x => x.ThenUserShouldHaveConfiguredRole())
            .And(x => x.ThenSuccessfulExecutionShouldBeRecorded())
            .BDDfy();
    }

    [Fact]
    public void EnabledRuleRunsAfterThirdPartyIsForceUnlinked()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenRegisteredUserExists())
            .And(x => x.GivenDiscordBindingExists())
            .And(x => x.GivenEnabledThirdPartyForceUnlinkedGrantRoleRuleExists())
            .When(x => x.WhenDiscordBindingIsForceUnlinked())
            .Then(x => x.ThenUserShouldHaveConfiguredRole())
            .And(x => x.ThenSuccessfulExecutionShouldBeRecorded())
            .BDDfy();
    }

    [Fact]
    public void FailedPasswordForceResetDoesNotRunRule()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenMissingUserId())
            .And(x => x.GivenEnabledPasswordForceResetGrantRoleRuleExists())
            .When(x => x.WhenMissingUserPasswordIsForceReset())
            .Then(x => x.ThenExecutionShouldNotBeRecorded())
            .BDDfy();
    }

    [Fact]
    public void FailedThirdPartyForceUnlinkDoesNotRunRule()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenRegisteredUserExists())
            .And(x => x.GivenEnabledThirdPartyForceUnlinkedGrantRoleRuleExists())
            .When(x => x.WhenMissingDiscordBindingIsForceUnlinked())
            .Then(x => x.ThenExecutionShouldNotBeRecorded())
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
    public void UnsupportedActionVersionIsRejected()
    {
        this.Given(x => x.GivenRoleExists())
            .When(x => x.WhenGrantRoleRuleWithUnsupportedVersionIsAdded())
            .Then(x => x.ThenRuleShouldBeRejected())
            .BDDfy();
    }

    [Fact]
    public void UnmatchedConditionSkipsConfiguredAction()
    {
        this.Given(x => x.GivenRoleExists())
            .And(x => x.GivenUnmatchedConditionRuleExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenUserShouldNotHaveConfiguredRole())
            .BDDfy();
    }

    [Fact]
    public void ContinuePolicyExecutesNextActionAfterFailure()
    {
        this.Given(x => x.GivenTwoRolesExist())
            .And(x => x.GivenContinueAfterFailureRuleExists())
            .And(x => x.GivenConfiguredRoleNoLongerExists())
            .When(x => x.WhenUserRegisters())
            .Then(x => x.ThenUserShouldHaveSecondConfiguredRole())
            .And(x => x.ThenFailedAndSuccessfulExecutionsShouldBeRecorded())
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
