using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Player;

public partial interface IEraService : IScope
{
    Task<GetEraConfigResponse> GetConfigAsync(
        GetEraConfigRequest request,
        CancellationToken cancellationToken);
    Task<GetEraPlayerResponse> GetPlayerAsync(
        GetEraPlayerRequest request,
        CancellationToken cancellationToken);
    Task<InitializeEraPlayerResponse> InitializePlayerAsync(
        InitializeEraPlayerCommand command,
        CancellationToken cancellationToken);
    Task<GetEraCharacterListResponse> GetCharacterListAsync(
        GetEraCharacterListRequest request,
        CancellationToken cancellationToken);
    Task<GetEraCharacterResponse> GetCharacterAsync(
        GetEraCharacterRequest request,
        CancellationToken cancellationToken);
    Task<DeleteEraCharacterResponse> DeleteCharacterAsync(
        DeleteEraCharacterCommand command,
        CancellationToken cancellationToken);
    Task<CreateEraCharacterResponse> CreateCharacterAsync(
        CreateEraCharacterCommand command,
        CancellationToken cancellationToken);
}

public partial class EraService : IEraService
{
    private readonly IEraDataProvider _dataProvider;
    private readonly IEraPlayerContext _playerContext;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    public EraService(
        IEraDataProvider dataProvider,
        IEraPlayerContext playerContext,
        ICurrentUser currentUser,
        IMapper mapper,
        ILocalizationService localizationService)
    {
        _dataProvider = dataProvider;
        _playerContext = playerContext;
        _currentUser = currentUser;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    private T Error<T>(
        HttpStatusCode status,
        string code,
        Expression<Func<EraContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        var result = new T
        {
            Code = status,
            Message = _localizationService.GetValue(text)
        };
        result.ErrorCode = code;
        return result;
    }

    private EraConfigDto? CreateConfigurationDto(
        IReadOnlyCollection<EraConfiguration> configurations)
    {
        if (configurations.Count == 0)
        {
            return null;
        }

        var configVersion = configurations.First().ConfigVersion;
        var hasMixedVersions = configurations.Any(
            configuration => configuration.ConfigVersion != configVersion);
        if (hasMixedVersions)
        {
            return null;
        }

        return new EraConfigDto
        {
            ConfigVersion = configVersion,
            Items = configurations
                .OrderBy(configuration => configuration.ConfigKey)
                .Select(configuration => _mapper.Map<EraConfigurationItemDto>(configuration))
                .ToList()
        };
    }

    private static bool TryGetIntegerConfiguration(
        IReadOnlyCollection<EraConfiguration> configurations,
        string configKey,
        out int value)
    {
        var configuration = configurations.SingleOrDefault(
            item => string.Equals(item.ConfigKey, configKey, StringComparison.Ordinal));
        if (configuration == null ||
            !string.Equals(configuration.ValueType, "integer", StringComparison.Ordinal))
        {
            value = default;
            return false;
        }

        return int.TryParse(
            configuration.Value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);
    }

    public async Task<GetEraConfigResponse> GetConfigAsync(
        GetEraConfigRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<GetEraConfigResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var configurations = await _dataProvider
            .GetConfigurationsAsync(false, cancellationToken)
            .ConfigureAwait(false);
        var config = CreateConfigurationDto(configurations);
        if (config == null)
        {
            return Error<GetEraConfigResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }

        return new GetEraConfigResponse
        {
            Code = HttpStatusCode.OK,
            Data = config
        };
    }

    public async Task<GetEraPlayerResponse> GetPlayerAsync(
        GetEraPlayerRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<GetEraPlayerResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext
            .RequireCurrentAsync(false, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraPlayerResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraPlayerDto>(player)
        };
    }

    public async Task<InitializeEraPlayerResponse> InitializePlayerAsync(
        InitializeEraPlayerCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue ||
            _currentUser.Id.Value == Guid.Empty ||
            !await _dataProvider.LockAccountAsync(_currentUser.Id.Value, cancellationToken).ConfigureAwait(false))
        {
            return Error<InitializeEraPlayerResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _dataProvider
            .LockPlayerByAccountIdAsync(_currentUser.Id.Value, cancellationToken)
            .ConfigureAwait(false);
        if (player != null)
        {
            return new InitializeEraPlayerResponse
            {
                Code = HttpStatusCode.OK,
                Data = _mapper.Map<EraPlayerDto>(player)
            };
        }

        var configurations = await _dataProvider
            .GetConfigurationsAsync(false, cancellationToken)
            .ConfigureAwait(false);
        var currentConfig = CreateConfigurationDto(configurations);
        if (currentConfig == null ||
            !TryGetIntegerConfiguration(
                configurations,
                EraConfigurationKeys.DefaultWarehouseCapacity,
                out var defaultWarehouseCapacity))
        {
            return Error<InitializeEraPlayerResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }

        player = new EraPlayer
        {
            Id = Guid.NewGuid(),
            WarehouseCapacity = defaultWarehouseCapacity,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var playerAccountLink = new EraPlayerAccountLink
        {
            Id = Guid.NewGuid(),
            AccountId = _currentUser.Id.Value,
            PlayerId = player.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _dataProvider.CreatePlayerAsync(player, cancellationToken).ConfigureAwait(false);
        await _dataProvider
            .CreatePlayerAccountLinkAsync(playerAccountLink, cancellationToken)
            .ConfigureAwait(false);
        return new InitializeEraPlayerResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraPlayerDto>(player)
        };
    }

    public async Task<GetEraCharacterListResponse> GetCharacterListAsync(
        GetEraCharacterListRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<GetEraCharacterListResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);

        var configurations = await _dataProvider.GetConfigurationsAsync(false, cancellationToken).ConfigureAwait(false);
        if (!TryGetIntegerConfiguration(configurations, EraConfigurationKeys.CharacterLimit, out var characterLimit) || characterLimit < 1)
        {
            return Error<GetEraCharacterListResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }

        var characters = await _dataProvider
            .GetCharactersByPlayerIdAsync(player.Id, cancellationToken)
            .ConfigureAwait(false);
        return new GetEraCharacterListResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<List<EraCharacterDto>>(characters),
            CharacterLimit = characterLimit
        };
    }

    public async Task<GetEraCharacterResponse> GetCharacterAsync(
        GetEraCharacterRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<GetEraCharacterResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);

        var character = await _dataProvider
            .GetCharacterByIdAsync(request.CharacterId, cancellationToken)
            .ConfigureAwait(false);
        if (character == null || character.PlayerId != player.Id)
        {
            return Error<GetEraCharacterResponse>(
                HttpStatusCode.NotFound,
                "era.character_not_found",
                context => context.CharacterNotFound);
        }

        return new GetEraCharacterResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraCharacterDto>(character)
        };
    }

    public async Task<CreateEraCharacterResponse> CreateCharacterAsync(
        CreateEraCharacterCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<CreateEraCharacterResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext
            .RequireCurrentAsync(true, cancellationToken)
            .ConfigureAwait(false);

        var existingCharacter = await _dataProvider
            .GetCharacterByCreationRequestIdAsync(player.Id, command.RequestId, cancellationToken)
            .ConfigureAwait(false);
        if (existingCharacter != null)
        {
            var requestMatches = existingCharacter.CharacterDefinitionId == command.CharacterDefinitionId &&
                                 existingCharacter.DisplayName == command.DisplayName;
            if (!requestMatches)
            {
                return Error<CreateEraCharacterResponse>(
                    HttpStatusCode.Conflict,
                    "era.idempotency_conflict",
                    context => context.IdempotencyConflict);
            }

            return new CreateEraCharacterResponse
            {
                Code = HttpStatusCode.OK,
                Data = _mapper.Map<EraCharacterDto>(existingCharacter)
            };
        }

        // 玩家行锁由当前命令事务持有，串行化同账号的创建和禁用操作。
        var configurations = await _dataProvider.GetConfigurationsAsync(false, cancellationToken).ConfigureAwait(false);
        if (!TryGetIntegerConfiguration(configurations, EraConfigurationKeys.CharacterLimit, out var characterLimit) || characterLimit < 1)
        {
            return Error<CreateEraCharacterResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.config_unavailable",
                context => context.ConfigUnavailable);
        }
        var characters = await _dataProvider.GetCharactersByPlayerIdAsync(player.Id, cancellationToken).ConfigureAwait(false);
        if (characters.Count(character => !character.IsDisabled) >= characterLimit)
        {
            return Error<CreateEraCharacterResponse>(
                HttpStatusCode.Conflict,
                "era.character_limit_reached",
                context => context.CharacterLimitReached);
        }

        var character = new EraCharacter
        {
            Id = Guid.NewGuid(),
            PlayerId = player.Id,
            CreationRequestId = command.RequestId,
            CharacterDefinitionId = command.CharacterDefinitionId,
            DisplayName = command.DisplayName,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _dataProvider.CreateCharacterAsync(character, cancellationToken).ConfigureAwait(false);
        return new CreateEraCharacterResponse
        {
            Code = HttpStatusCode.OK,
            Data = _mapper.Map<EraCharacterDto>(character)
        };
    }

    public async Task<DeleteEraCharacterResponse> DeleteCharacterAsync(
        DeleteEraCharacterCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            return Error<DeleteEraCharacterResponse>(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                context => context.Unauthorized);
        }

        var player = await _playerContext
            .RequireCurrentAsync(true, cancellationToken)
            .ConfigureAwait(false);

        var character = await _dataProvider
            .LockCharacterAsync(command.CharacterId, cancellationToken)
            .ConfigureAwait(false);
        if (character == null || character.PlayerId != player.Id)
        {
            return Error<DeleteEraCharacterResponse>(
                HttpStatusCode.NotFound,
                "era.character_not_found",
                context => context.CharacterNotFound);
        }

        if (character.IsDisabled)
        {
            return new DeleteEraCharacterResponse
            {
                Code = HttpStatusCode.OK,
                Data = character.Id
            };
        }

        character.IsDisabled = true;
        character.Version++;
        return new DeleteEraCharacterResponse
        {
            Code = HttpStatusCode.OK,
            Data = character.Id
        };
    }

}
