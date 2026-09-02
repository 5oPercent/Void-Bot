using Discord;
using Discord.WebSocket;
using System.Net;
using System.Text.Json.Serialization;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Core.Services.Discord.Context;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Discord;
using Void.Bot.Message.Constants.Discord;
using Void.Bot.Message.Dto.Discord;
using Void.Bot.Message.Dto.HttpClient;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Services.Discord;

public partial interface IDiscordService : IScope
{
    Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken);
    
    Task<DiscordStopBotResponse> StopAsync(DiscordStopBotRequest request, CancellationToken cancellationToken);
    
    Task ApplyGlobalSlashCommandAsync(DiscordApplyGlobalSlashCommandRequest request, CancellationToken cancellationToken);

    Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken);

    Task<GetDiscordManagedGuildListResponse> GetManagedGuildListAsync(
        GetDiscordManagedGuildListRequest request,
        CancellationToken cancellationToken);

    Task<GetDiscordGuildChannelListResponse> GetGuildChannelListAsync(
        GetDiscordGuildChannelListRequest request,
        CancellationToken cancellationToken);
}

public partial class DiscordService : IDiscordService
{
    private const string ManagedGuildCacheKeyPrefix = "discord:managed-guilds:";
    private static readonly TimeSpan ManagedGuildCacheDuration = TimeSpan.FromMinutes(1);

    private readonly DiscordSetting _setting;
    private readonly IDiscordDataProvider _dataProvider;
    private readonly IDiscordVoiceChannelDataProvider _voiceChannelDataProvider;
    private readonly IDiscordBindingContext _bindingContext;
    private readonly IDiscordGuildContext _guildContext;
    private readonly IDiscordSlashCommandRoute _slashCommandRoute;
    private readonly IVoidBotHttpClientFactory _httpClientFactory;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger _logger;
    
    private readonly DiscordSocketClient _client;

    public DiscordService(
        DiscordSetting setting,
        IDiscordDataProvider dataProvider,
        IDiscordVoiceChannelDataProvider voiceChannelDataProvider,
        IDiscordBindingContext bindingContext,
        IDiscordGuildContext guildContext,
        IDiscordSlashCommandRoute slashCommandRoute,
        IVoidBotHttpClientFactory httpClientFactory,
        ICacheManager cacheManager,
        ILogger logger,
        DiscordSocketClient client)
    {
        _setting = setting;
        _dataProvider = dataProvider;
        _voiceChannelDataProvider = voiceChannelDataProvider;
        _bindingContext = bindingContext;
        _guildContext = guildContext;
        _slashCommandRoute = slashCommandRoute; 
        _httpClientFactory = httpClientFactory;
        _cacheManager = cacheManager;
        _logger = logger;
        _client = client;
    }

    public async Task<DiscordStartBotResponse> StartAsync(DiscordStartBotRequest request, CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _setting.Token).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);

        return new DiscordStartBotResponse();
    }

    public async Task<DiscordStopBotResponse> StopAsync(DiscordStopBotRequest request, CancellationToken cancellationToken)
    {
        if (_client.ConnectionState != ConnectionState.Disconnected)
        {
            await _client.StopAsync().ConfigureAwait(false);
        }

        return new DiscordStopBotResponse();
    }

    public async Task ApplyGlobalSlashCommandAsync(DiscordApplyGlobalSlashCommandRequest request ,CancellationToken cancellationToken)
    {
        var commands = _slashCommandRoute.GetGlobalCommands().Cast<ApplicationCommandProperties>().ToArray();
        var requestOptions = new RequestOptions { CancelToken = cancellationToken };

        await _client.BulkOverwriteGlobalApplicationCommandsAsync(commands, requestOptions).ConfigureAwait(false);
    }
    
    public async Task ApplyGuildSlashCommandAsync(DiscordApplyGuildSlashCommandRequest request, CancellationToken cancellationToken)
    {
        var guild = _client.GetGuild(request.GuildId);
        if (guild == null) return;

        var commands = _slashCommandRoute.GetGuildCommands([request.GuildId]).Cast<ApplicationCommandProperties>().ToArray();
        var requestOptions = new RequestOptions { CancelToken = cancellationToken };

        await guild.BulkOverwriteApplicationCommandAsync(commands, requestOptions).ConfigureAwait(false);
    }

    public async Task<GetDiscordManagedGuildListResponse> GetManagedGuildListAsync(
        GetDiscordManagedGuildListRequest request,
        CancellationToken cancellationToken)
    {
        var response = new GetDiscordManagedGuildListResponse();
        var cacheKey = $"{ManagedGuildCacheKeyPrefix}{_bindingContext.Binding.UserId}";
        var cachedGuilds = await _cacheManager
            .Get<List<DiscordManagedGuildDto>>(cacheKey, cancellationToken)
            .ConfigureAwait(false);
        if (cachedGuilds != null)
        {
            response.Data = cachedGuilds;
            return response;
        }

        var guildResponse = await _httpClientFactory.GetAsync<List<DiscordUserGuildApiDto>>(
            DiscordConstants.CurrentUserGuildsUrl,
            cancellationToken,
            tokenModel: new TokenModel { Parameter = _bindingContext.Binding.AccessToken }).ConfigureAwait(false);
        if ((int)guildResponse.Code is < 200 or >= 300 || guildResponse.Data == null)
        {
            response.Code = (int)guildResponse.Code == 0 ? HttpStatusCode.BadGateway : guildResponse.Code;
            response.Message = "Failed to read Discord guilds. Please authorize Discord again.";
            return response;
        }

        response.Data = guildResponse.Data
            .Where(x => x.Owner || HasGuildManagementPermission(x.Permissions))
            .Select(x =>
            {
                var installed = ulong.TryParse(x.Id, out var guildId) && _client.GetGuild(guildId) != null;
                return new DiscordManagedGuildDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IconUrl = string.IsNullOrWhiteSpace(x.Icon)
                        ? null
                        : $"https://cdn.discordapp.com/icons/{x.Id}/{x.Icon}.png",
                    IsOwner = x.Owner,
                    IsBotInstalled = installed,
                    CanManageGuild = true
                };
            })
            .ToList();

        await _cacheManager
            .Set(cacheKey, response.Data!, ManagedGuildCacheDuration, cancellationToken)
            .ConfigureAwait(false);

        return response;
    }

    public async Task<GetDiscordGuildChannelListResponse> GetGuildChannelListAsync(
        GetDiscordGuildChannelListRequest request,
        CancellationToken cancellationToken)
    {
        var response = new GetDiscordGuildChannelListResponse();
        var guildId = _guildContext.GuildId;
        var guild = _guildContext.Guild;

        var configs = await _voiceChannelDataProvider.GetConfigsAsync(guildId, cancellationToken).ConfigureAwait(false);
        var configuredChannels = configs
            .SelectMany(x => new[]
            {
                (ChannelId: x.TriggerChannelId, ConfigId: (Guid?)x.Id),
                (ChannelId: x.CreateCategoryId, ConfigId: (Guid?)x.Id)
            })
            .GroupBy(x => x.ChannelId)
            .ToDictionary(x => x.Key, x => x.First().ConfigId);

        response.Data = guild.Channels
            .Where(x => x.ChannelType is ChannelType.Text or ChannelType.News or ChannelType.Voice or ChannelType.Category)
            .OrderBy(x => x.Position)
            .Select(x => new DiscordGuildChannelDto
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Type = x.ChannelType.ToString(),
                ParentId = x is INestedChannel nestedChannel && nestedChannel.CategoryId.HasValue
                    ? nestedChannel.CategoryId.Value.ToString()
                    : null,
                Position = x.Position,
                UsableAsTrigger = x.ChannelType == ChannelType.Voice,
                UsableAsCreateCategory = x.ChannelType == ChannelType.Category,
                UsableAsRolePanel = x.ChannelType is ChannelType.Text or ChannelType.News,
                Configured = configuredChannels.ContainsKey(x.Id),
                ConfigId = configuredChannels.GetValueOrDefault(x.Id)
            })
            .ToList();

        return response;
    }

    private static bool HasGuildManagementPermission(string? permissions)
    {
        return ulong.TryParse(permissions, out var value) && (value & (0x8 | 0x20)) != 0;
    }

    private sealed class DiscordUserGuildApiDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("icon")]
        public string? Icon { get; init; }

        [JsonPropertyName("owner")]
        public bool Owner { get; init; }

        [JsonPropertyName("permissions")]
        public string? Permissions { get; init; }
    }
}
