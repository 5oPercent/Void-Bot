using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Services.Jobs;
using Void.Bot.Core.Settings.Bungie;

namespace Void.Bot.Core.Services.Sync;

public partial interface ISyncService : IScope
{
}

public partial class SyncService : ISyncService
{
    private IVoidBotBackgroundJobClient _backgroundJob;
    private ICacheManager _cacheManager;
    private IVoidBotHttpClientFactory _httpClientFactory;
    private IBungieDataProvider _bungieDataProvider;
    private Destiny2Setting _destiny2Setting;

    public SyncService(
        IVoidBotBackgroundJobClient backgroundJob,
        ICacheManager cacheManager,
        IVoidBotHttpClientFactory httpClientFactory,
        IBungieDataProvider bungieDataProvider,
        Destiny2Setting destiny2Setting)
    {
        _backgroundJob = backgroundJob;
        _cacheManager = cacheManager;
        _httpClientFactory = httpClientFactory;
        _bungieDataProvider = bungieDataProvider;
        _destiny2Setting = destiny2Setting;
    }
}
