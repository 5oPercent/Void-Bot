using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraService
{
    Task<GetEraAdminPlayerResponse> GetAdminPlayerAsync(
        GetEraAdminPlayerRequest request,
        CancellationToken cancellationToken);
    Task<UpdateEraConfigResponse> UpdateConfigAsync(
        UpdateEraConfigCommand command,
        CancellationToken cancellationToken);
    Task<SetEraWarehouseCapacityResponse> SetCapacityAsync(
        SetEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken);
    Task<AdjustEraWarehouseCapacityResponse> AdjustCapacityAsync(
        AdjustEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken);
}

public partial class EraService
{
    public async Task<GetEraAdminPlayerResponse> GetAdminPlayerAsync(
        GetEraAdminPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var player = await _playerContext
            .RequireTargetAsync(request.PlayerId, false, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraAdminPlayerResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraPlayerDto>(player)
        };
    }

    public async Task<UpdateEraConfigResponse> UpdateConfigAsync(
        UpdateEraConfigCommand command,
        CancellationToken cancellationToken)
    {
        var configurations = await _dataProvider
            .GetConfigurationsAsync(true, cancellationToken)
            .ConfigureAwait(false);
        var currentConfig = CreateConfigurationDto(configurations);
        if (currentConfig == null)
        {
            return Error<UpdateEraConfigResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }

        if (currentConfig.ConfigVersion != command.ExpectedVersion)
        {
            return Error<UpdateEraConfigResponse>(
                HttpStatusCode.Conflict,
                "era.version_conflict",
                context => context.VersionConflict);
        }

        var nextVersion = currentConfig.ConfigVersion + 1;
        var incomingItems = command.Config.Items.ToDictionary(
            item => item.Key,
            StringComparer.Ordinal);
        var existingItems = configurations.ToDictionary(
            configuration => configuration.ConfigKey,
            StringComparer.Ordinal);
        var removedConfigurations = configurations
            .Where(configuration => !incomingItems.ContainsKey(configuration.ConfigKey))
            .ToList();
        if (removedConfigurations.Count > 0)
        {
            _dataProvider.RemoveConfigurations(removedConfigurations);
        }

        var publishedConfigurations = new List<EraConfiguration>();
        var createdConfigurations = new List<EraConfiguration>();
        foreach (var item in command.Config.Items)
        {
            EraConfiguration configuration;
            if (existingItems.TryGetValue(item.Key, out var existingConfiguration))
            {
                configuration = existingConfiguration;
            }
            else
            {
                configuration = new EraConfiguration
                {
                    Id = Guid.NewGuid()
                };
                createdConfigurations.Add(configuration);
            }

            configuration.ConfigKey = item.Key;
            configuration.ValueType = item.ValueType;
            configuration.Value = item.Value;
            configuration.Description = item.Description;
            configuration.ConfigVersion = nextVersion;
            configuration.LastModifiedBy = _currentUser.Id;
            publishedConfigurations.Add(configuration);
        }

        if (createdConfigurations.Count > 0)
        {
            await _dataProvider
                .CreateConfigurationsAsync(createdConfigurations, cancellationToken)
                .ConfigureAwait(false);
        }

        var publishedConfig = CreateConfigurationDto(publishedConfigurations);
        if (publishedConfig == null)
        {
            return Error<UpdateEraConfigResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }

        return new UpdateEraConfigResponse
        {
            Code = HttpStatusCode.OK,
            Data = publishedConfig
        };
    }

    public async Task<SetEraWarehouseCapacityResponse> SetCapacityAsync(
        SetEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        return await ChangeCapacityAsync<SetEraWarehouseCapacityResponse>(
                command.PlayerId,
                command.RequestId,
                command.ExpectedVersion,
                command.Capacity,
                false,
                command.Reason,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AdjustEraWarehouseCapacityResponse> AdjustCapacityAsync(
        AdjustEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        return await ChangeCapacityAsync<AdjustEraWarehouseCapacityResponse>(
                command.PlayerId,
                command.RequestId,
                command.ExpectedVersion,
                command.Delta,
                true,
                command.Reason,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<T> ChangeCapacityAsync<T>(
        Guid playerId,
        Guid requestId,
        long expectedVersion,
        int value,
        bool adjustment,
        string reason,
        CancellationToken cancellationToken)
        where T : EraResponse<EraCapacityReceiptDto>, new()
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<T>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext.RequireTargetAsync(playerId, true, cancellationToken).ConfigureAwait(false);

        var receipt = await _dataProvider.GetReceiptAsync(playerId, requestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.OperatorId != _currentUser.Id.Value ||
                receipt.IsAdjustment != adjustment ||
                receipt.RequestedValue != value ||
                receipt.ExpectedVersion != expectedVersion ||
                receipt.Reason != reason)
            {
                return Error<T>(
                    HttpStatusCode.Conflict,
                    "era.idempotency_conflict",
                    context => context.IdempotencyConflict);
            }

            return new T
            {
                Code = HttpStatusCode.OK,
                Data = _mapper.Map<EraCapacityReceiptDto>(receipt)
            };
        }

        if (player.Version != expectedVersion)
        {
            return Error<T>(
                HttpStatusCode.Conflict,
                "era.version_conflict",
                context => context.VersionConflict);
        }

        long nextCapacity;
        if (adjustment)
        {
            nextCapacity = (long)player.WarehouseCapacity + value;
        }
        else
        {
            nextCapacity = value;
        }

        if (nextCapacity < 0 || nextCapacity > int.MaxValue)
        {
            return Error<T>(
                HttpStatusCode.BadRequest,
                "era.capacity_out_of_range",
                context => context.CapacityOutOfRange);
        }

        receipt = new EraCapacityChange
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            RequestId = requestId,
            OperatorId = _currentUser.Id.Value,
            IsAdjustment = adjustment,
            RequestedValue = value,
            ExpectedVersion = expectedVersion,
            Reason = reason,
            PreviousCapacity = player.WarehouseCapacity,
            WarehouseCapacity = (int)nextCapacity,
            PlayerVersion = player.Version + 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        player.WarehouseCapacity = (int)nextCapacity;
        player.Version++;
        await _dataProvider.CreateReceiptAsync(receipt, cancellationToken).ConfigureAwait(false);
        return new T
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraCapacityReceiptDto>(receipt)
        };
    }
}
