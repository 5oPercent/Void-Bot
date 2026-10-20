using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

/// <summary>已持有队伍与世界锁的统一结束转换；不生成资产、不删除装备或历史记录。</summary>
public static class EraWorldLifecycle
{
    public static void Finish(EraWorld world, EraWorldTeamState teamState,
        IReadOnlyList<EraWorldMember> members, EraWorldEndReason reason, DateTimeOffset now)
    {
        if (world.State is EraWorldState.Ended or EraWorldState.BuildFailed) return;
        // 当前无地面资产，结束及活动标记释放在同一事务完成；未来分批清理从 Ending 扩展。
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
