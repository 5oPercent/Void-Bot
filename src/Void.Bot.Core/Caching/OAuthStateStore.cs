using System.Security.Cryptography;
using System.Text.Json;
using Serilog;
using Void.Bot.Core.Services;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Caching;

public interface IOAuthStateStore : IScope
{
    Task<CreateOAuthStateResponse> CreateAsync(
        CreateOAuthStateRequest request,
        CancellationToken cancellationToken = default);

    Task<TakeOAuthStateResponse> TakeAsync(
        TakeOAuthStateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class OAuthStateStore : IOAuthStateStore
{
    private const int StateByteLength = 32;
    private const int EncodedStateLength = 43;
    private const string KeyPrefix = "oauth:state:";
    private readonly ICacheManager _cacheManager;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;

    public OAuthStateStore(ICacheManager cacheManager, TimeProvider timeProvider, ILogger logger)
    {
        _cacheManager = cacheManager;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CreateOAuthStateResponse> CreateAsync(
        CreateOAuthStateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Platform == AppPlatformEnum.Void ||
            !Enum.IsDefined(request.Platform) ||
            request.TimeToLive <= TimeSpan.Zero ||
            request.Flow switch
            {
                OAuthStateFlow.Login => request.InitiatingAccountId is not null,
                OAuthStateFlow.Link => request.InitiatingAccountId is null ||
                                       request.InitiatingAccountId == Guid.Empty,
                _ => true
            })
        {
            _logger.Warning(
                "OAuth state creation rejected: {Platform}, flow {Flow}, account {AccountId}, TTL {TimeToLive}",
                request.Platform,
                request.Flow,
                request.InitiatingAccountId,
                request.TimeToLive);
            return new CreateOAuthStateResponse.Rejected();
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(StateByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var record = new OAuthStateRecord
        {
            State = state,
            Platform = request.Platform,
            Flow = request.Flow,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.Add(request.TimeToLive),
            InitiatingAccountId = request.InitiatingAccountId
        };

        try
        {
            await _cacheManager.Set(
                $"{KeyPrefix}{state}",
                record,
                request.TimeToLive,
                cancellationToken).ConfigureAwait(false);

            _logger.Information(
                "OAuth state stored: {Platform}, flow {Flow}, expires at {ExpiresAt}",
                record.Platform,
                record.Flow,
                record.ExpiresAt);
            return new CreateOAuthStateResponse.Created(record);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(
                exception,
                "OAuth state creation failed because the state store is unavailable: {Platform}, flow {Flow}",
                request.Platform,
                request.Flow);
            return new CreateOAuthStateResponse.Unavailable();
        }
    }

    public async Task<TakeOAuthStateResponse> TakeAsync(
        TakeOAuthStateRequest request,
        CancellationToken cancellationToken = default)
    {
        var state = request.State;
        if (string.IsNullOrEmpty(state) ||
            state.Length != EncodedStateLength ||
            state.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            _logger.Warning("OAuth state consumption rejected malformed input");
            return new TakeOAuthStateResponse.Missing();
        }

        try
        {
            var record = await _cacheManager.Take<OAuthStateRecord>(
                $"{KeyPrefix}{state}",
                cancellationToken).ConfigureAwait(false);

            if (record is null)
            {
                _logger.Warning("OAuth state was missing, expired, or already consumed");
                return new TakeOAuthStateResponse.Missing();
            }

            var now = _timeProvider.GetUtcNow();
            var isValid = record.State == state &&
                          record.Platform != AppPlatformEnum.Void &&
                          Enum.IsDefined(record.Platform) &&
                          (record.Flow switch
                          {
                              OAuthStateFlow.Login => record.InitiatingAccountId is null,
                              OAuthStateFlow.Link => record.InitiatingAccountId is not null &&
                                                     record.InitiatingAccountId != Guid.Empty,
                              _ => false
                          }) &&
                          record.IssuedAt <= now &&
                          record.ExpiresAt > now &&
                          record.ExpiresAt > record.IssuedAt;
            if (!isValid)
            {
                _logger.Warning(
                    "OAuth state record failed validation: {Platform}, flow {Flow}, issued at {IssuedAt}, expires at {ExpiresAt}",
                    record.Platform,
                    record.Flow,
                    record.IssuedAt,
                    record.ExpiresAt);
                return new TakeOAuthStateResponse.Missing();
            }

            _logger.Information(
                "OAuth state consumed: {Platform}, flow {Flow}, account {AccountId}",
                record.Platform,
                record.Flow,
                record.InitiatingAccountId);
            return new TakeOAuthStateResponse.Found(record);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            _logger.Warning(exception, "OAuth state record could not be deserialized");
            return new TakeOAuthStateResponse.Missing();
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth state consumption failed because the state store is unavailable");
            return new TakeOAuthStateResponse.Unavailable();
        }
    }
}
