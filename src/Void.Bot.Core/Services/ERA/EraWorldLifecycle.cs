using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

/// <summary>调用方在队伍与世界锁内完成未拾取资产失效后执行的统一结束转换，保留历史记录。</summary>
public static class EraWorldLifecycle
{
    public static void Finish(EraWorld world, EraWorldTeamState teamState,
        IReadOnlyList<EraWorldMember> members, EraWorldEndReason reason, DateTimeOffset now)
    {
        if (world.State is EraWorldState.Ended or EraWorldState.BuildFailed) return;
        // 未拾取资产失效、终态及活动标记释放由调用方放在同一事务中提交。
        world.State = reason == EraWorldEndReason.ConstructionFailed ? EraWorldState.BuildFailed : EraWorldState.Ended;
        world.EndReason = reason;
        world.EndedAt = now;
        world.TimeoutObservedAt = null;
        if (teamState.ActiveWorldId == world.Id) teamState.ActiveWorldId = null;
        foreach (var member in members)
        {
            member.IsContinuing = false;
            member.ActivePlayerId = null;
            member.ActiveCharacterId = null;
        }
        world.Version = checked(world.Version + 1);
    }
}
