using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Sync")]
public sealed class SyncEndpointContextTemplate : IContextTemplate
{
    public string GetDestiny2ManifestSyncOptions { get; set; } = "获取 Destiny 2 清单同步选项";
    public string SynchronizeDestiny2Manifest { get; set; } = "同步 Destiny 2 清单";

}
