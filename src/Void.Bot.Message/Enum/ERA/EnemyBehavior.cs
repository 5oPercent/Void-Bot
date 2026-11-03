namespace Void.Bot.Message.Enum.ERA;

/// <summary>敌人的默认非战斗行为；驻守点和巡逻路线由地图实例提供。</summary>
public enum EnemyBehavior
{
    /// <summary>驻守地图实例指定的位置。</summary>
    Guard = 0,
    /// <summary>沿地图实例提供的路线巡逻。</summary>
    Patrol = 1,
    /// <summary>自主选择可达目的地漫游，不要求绑定区域。</summary>
    Roam = 2
}
