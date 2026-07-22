using System.Security.Cryptography;
using System.Text.Json;
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

    public OAuthStateStore(ICacheManager cacheManager, TimeProvider timeProvider)
    {
        _cacheManager = cacheManager;
        _timeProvider = timeProvider;
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

            return new CreateOAuthStateResponse.Created(record);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
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
            return new TakeOAuthStateResponse.Missing();
        }

        try
        {
            var record = await _cacheManager.Take<OAuthStateRecord>(
                $"{KeyPrefix}{state}",
                cancellationToken).ConfigureAwait(false);

            var now = _timeProvider.GetUtcNow();
            return record is not null &&
                   record.State == state &&
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
                   record.ExpiresAt > record.IssuedAt
                ? new TakeOAuthStateResponse.Found(record)
                : new TakeOAuthStateResponse.Missing();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException)
        {
            return new TakeOAuthStateResponse.Missing();
        }
        catch
        {
            return new TakeOAuthStateResponse.Unavailable();
        }
    }
}
