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

    public EventActionDefinitionDto Definition => new()
    {
        Type = ActionType,
        DisplayName = "授权角色",
        Description = "为事件关联的用户授予一个或多个角色。",
        SupportedEvents = [BuiltInEventNames.AccountRegistered],
        Arguments =
        [
            new EventActionArgumentDefinitionDto
            {
                Name = "RoleIds",
                DisplayName = "角色",
                Description = "选择需要自动授予的角色。",
                Required = true,
                Multiple = true,
                ValueSource = "Role",
                DataSourceEndpoint = "/api/security/role/tree",
                DataSourceValueField = "id",
                DataSourceLabelField = "name"
            },
            new EventActionArgumentDefinitionDto
            {
                Name = "DurationDays",
                DisplayName = "授权天数",
                Description = "留空表示永久有效；填写 1 至 36500 的整数表示授权有效天数。",
                Required = false,
                Multiple = false,
                ValueSource = "Number"
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
        if (builtInEvent is not AccountRegisteredEvent accountRegistered)
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' does not support event '{builtInEvent.Name}'.");
        }

        var (roles, durationDays, error) = await ResolveArgumentsAsync(
            arguments, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException(error);
        }

        var resolvedRoleIds = roles.Select(x => x.Id).ToHashSet();
        var existingRoleIds = await userRoleRepository.QueryNoTracking()
            .Where(x => x.UserId == accountRegistered.UserId && resolvedRoleIds.Contains(x.RoleId))
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        existingRoleIds.AddRange(dbContext.ChangeTracker.Entries<UserRole>()
            .Where(x => x.State != EntityState.Deleted && x.Entity.UserId == accountRegistered.UserId)
            .Select(x => x.Entity.RoleId));

        var deadline = durationDays.HasValue
            ? DateTimeOffset.UtcNow.AddDays(durationDays.Value)
            : (DateTimeOffset?)null;
        var newUserRoles = resolvedRoleIds.Except(existingRoleIds)
            .Select(roleId => new UserRole
            {
                UserId = accountRegistered.UserId,
                RoleId = roleId,
                Deadline = deadline
            })
            .ToList();

        if (newUserRoles.Count != 0)
        {
            await userRoleRepository.InsertManyAsync(newUserRoles, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<(List<Role> Roles, int? DurationDays, string? Error)> ResolveArgumentsAsync(
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        arguments.TryGetValue("RoleIds", out var configuredRoleIds);
        var unsupportedArguments = arguments.Keys
            .Where(x => !string.Equals(x, "RoleIds", StringComparison.Ordinal) &&
                        !string.Equals(x, "DurationDays", StringComparison.Ordinal))
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

        arguments.TryGetValue("DurationDays", out var configuredDurationDays);
        var durationValues = (configuredDurationDays ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        if (durationValues.Count > 1 ||
            durationValues.Count == 1 &&
            (!int.TryParse(durationValues[0], out var parsedDurationDays) ||
             parsedDurationDays is < 1 or > 36500))
        {
            return ([], null,
                $"Event action '{ActionType}' DurationDays must be one integer between 1 and 36500.");
        }

        var durationDays = durationValues.Count == 0
            ? (int?)null
            : int.Parse(durationValues[0]);

        var roles = await roleRepository.QueryNoTracking()
            .Where(x => roleIds.Contains(x.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var missingRoleIds = roleIds.Except(roles.Select(x => x.Id)).ToList();
        if (missingRoleIds.Count != 0)
        {
            return ([], null,
                $"Event action '{ActionType}' references roles that do not exist: {string.Join(", ", missingRoleIds)}.");
        }

        return (roles, durationDays, null);
    }
}
