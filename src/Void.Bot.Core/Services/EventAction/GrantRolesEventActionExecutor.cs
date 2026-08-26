using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public class GrantRolesEventActionExecutor(
    VoidBotDbContext dbContext,
    IRepository<Role> roleRepository,
    IRepository<UserRole> userRoleRepository,
    ICurrentUser currentUser,
    IUserRoleService userRoleService) : IConfiguredEventActionExecutor
{
    public const string ActionType = "GrantRoles";

    public string Type => ActionType;

    public int Version => 1;

    public EventActionDefinitionDto Definition => new()
    {
        Type = ActionType,
        Version = Version,
        DisplayName = "授权角色",
        Description = "为事件关联的用户授予一个或多个角色。",
        SupportedEvents =
        [
            BuiltInEventNames.AccountRegistered,
            BuiltInEventNames.AccountPasswordForceReset,
            BuiltInEventNames.AccountThirdPartyForceUnlinked
        ],
        Arguments =
        [
            new EventActionArgumentDefinitionDto
            {
                Name = "RoleIds",
                DisplayName = "角色",
                Description = "选择需要自动授予的角色。",
                ValueType = "Guid",
                Required = true,
                Multiple = true,
                MinLength = 1,
                ValueSource = "Role",
                DataSourceEndpoint = "/api/security/role/tree",
                DataSourceValueField = "id",
                DataSourceLabelField = "name"
            },
            new EventActionArgumentDefinitionDto
            {
                Name = "Duration",
                DisplayName = "授权时长",
                Description = "留空表示永久有效；填写时必须同时选择时间单位。",
                ValueType = "Integer",
                Required = false,
                Multiple = false,
                Minimum = 1,
                MaximumDependsOn = "DurationUnit",
                MaximumByOption = new Dictionary<string, long>
                {
                    ["Second"] = 3153600000L,
                    ["Minute"] = 52560000L,
                    ["Hour"] = 876000L,
                    ["Day"] = 36500L
                },
                RequiredWith = ["DurationUnit"],
                ValueSource = "Number"
            },
            new EventActionArgumentDefinitionDto
            {
                Name = "DurationUnit",
                DisplayName = "时间单位",
                Description = "支持秒、分钟、小时和天。",
                ValueType = "String",
                Required = false,
                Multiple = false,
                RequiredWith = ["Duration"],
                ValueSource = "Options",
                Options =
                [
                    new EventActionArgumentOptionDto { Value = "Second", Label = "秒" },
                    new EventActionArgumentOptionDto { Value = "Minute", Label = "分钟" },
                    new EventActionArgumentOptionDto { Value = "Hour", Label = "小时" },
                    new EventActionArgumentOptionDto { Value = "Day", Label = "天" }
                ]
            }
        ]
    };

    public async Task<string?> ValidateAsync(
        string eventName,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        if (!Definition.SupportedEvents.Contains(eventName, StringComparer.OrdinalIgnoreCase))
        {
            return $"Event action '{ActionType}' does not support event '{eventName}'.";
        }

        var (roles, _, error) = await ResolveArgumentsAsync(
            arguments, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(error))
        {
            return error;
        }

        if (roles.Any(x => x.Id == RoleConstants.SuperAdminId) &&
            (!currentUser.Id.HasValue ||
             !await userRoleService.CheckUserIsSuperAdminAsync(
                 currentUser.Id.Value, cancellationToken).ConfigureAwait(false)))
        {
            return "Only an active SuperAdmin can configure automatic SuperAdmin grants.";
        }

        return null;
    }

    public async Task ExecuteAsync(
        IBuiltInEvent builtInEvent,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        if (builtInEvent is not IUserBuiltInEvent userEvent)
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' does not support event '{builtInEvent.Name}'.");
        }

        var (roles, duration, error) = await ResolveArgumentsAsync(
            arguments, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException(error);
        }

        var resolvedRoleIds = roles.Select(x => x.Id).ToHashSet();
        var existingRoleIds = await userRoleRepository.QueryNoTracking()
            .Where(x => x.UserId == userEvent.UserId && resolvedRoleIds.Contains(x.RoleId))
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        existingRoleIds.AddRange(dbContext.ChangeTracker.Entries<UserRole>()
            .Where(x => x.State != EntityState.Deleted && x.Entity.UserId == userEvent.UserId)
            .Select(x => x.Entity.RoleId));

        var deadline = duration.HasValue
            ? DateTimeOffset.UtcNow.Add(duration.Value)
            : (DateTimeOffset?)null;
        var newUserRoles = resolvedRoleIds.Except(existingRoleIds)
            .Select(roleId => new UserRole
            {
                UserId = userEvent.UserId,
                RoleId = roleId,
                Deadline = deadline
            })
            .ToList();

        if (newUserRoles.Count != 0)
        {
            await userRoleRepository.InsertManyAsync(newUserRoles, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<(List<Role> Roles, TimeSpan? Duration, string? Error)> ResolveArgumentsAsync(
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        arguments.TryGetValue("RoleIds", out var configuredRoleIds);
        var unsupportedArguments = arguments.Keys
            .Where(x => !string.Equals(x, "RoleIds", StringComparison.Ordinal) &&
                        !string.Equals(x, "Duration", StringComparison.Ordinal) &&
                        !string.Equals(x, "DurationUnit", StringComparison.Ordinal))
            .ToList();
        if (unsupportedArguments.Count != 0)
        {
            return ([], null,
                $"Event action '{ActionType}' contains unsupported arguments: {string.Join(", ", unsupportedArguments)}.");
        }

        var invalidRoleIds = new List<string>();
        var roleIds = new HashSet<Guid>();
        foreach (var configuredRoleId in configuredRoleIds ?? [])
        {
            if (string.IsNullOrWhiteSpace(configuredRoleId))
            {
                continue;
            }

            if (Guid.TryParse(configuredRoleId, out var roleId))
            {
                roleIds.Add(roleId);
            }
            else
            {
                invalidRoleIds.Add(configuredRoleId);
            }
        }

        if (invalidRoleIds.Count != 0)
        {
            return ([], null,
                $"Event action '{ActionType}' contains invalid role IDs: {string.Join(", ", invalidRoleIds)}.");
        }

        if (roleIds.Count == 0)
        {
            return ([], null, $"Event action '{ActionType}' requires at least one RoleId.");
        }

        arguments.TryGetValue("Duration", out var configuredDurations);
        arguments.TryGetValue("DurationUnit", out var configuredDurationUnits);
        var durationValues = (configuredDurations ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        var durationUnitValues = (configuredDurationUnits ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        TimeSpan? duration = null;
        if (durationValues.Count != 0 || durationUnitValues.Count != 0)
        {
            if (durationValues.Count != 1 ||
                !long.TryParse(durationValues.FirstOrDefault(), out var durationValue) ||
                durationValue < 1)
            {
                return ([], null,
                    $"Event action '{ActionType}' Duration must be one positive integer.");
            }

            if (durationUnitValues.Count != 1)
            {
                return ([], null,
                    $"Event action '{ActionType}' DurationUnit is required when Duration is configured.");
            }

            var durationUnit = durationUnitValues[0];
            var maximumValue = durationUnit.ToUpperInvariant() switch
            {
                "SECOND" => 3153600000L,
                "MINUTE" => 52560000L,
                "HOUR" => 876000L,
                "DAY" => 36500L,
                _ => 0L
            };
            if (maximumValue == 0)
            {
                return ([], null,
                    $"Event action '{ActionType}' DurationUnit must be Second, Minute, Hour, or Day.");
            }

            if (durationValue > maximumValue)
            {
                return ([], null,
                    $"Event action '{ActionType}' Duration cannot exceed 36500 days.");
            }

            duration = durationUnit.ToUpperInvariant() switch
            {
                "SECOND" => TimeSpan.FromSeconds(durationValue),
                "MINUTE" => TimeSpan.FromMinutes(durationValue),
                "HOUR" => TimeSpan.FromHours(durationValue),
                _ => TimeSpan.FromDays(durationValue)
            };
        }

        var roles = await roleRepository.QueryNoTracking()
            .Where(x => roleIds.Contains(x.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var missingRoleIds = roleIds.Except(roles.Select(x => x.Id)).ToList();
        if (missingRoleIds.Count != 0)
        {
            return ([], null,
                $"Event action '{ActionType}' references roles that do not exist: {string.Join(", ", missingRoleIds)}.");
        }

        return (roles, duration, null);
    }
}
