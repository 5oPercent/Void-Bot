namespace Void.Bot.Message.Enum.ERA;

/// <summary>敌人进入交战的攻击倾向，与默认非战斗行为独立。</summary>
public enum EnemyAggression
{
    /// <summary>发现敌对目标后主动进入交战。</summary>
    Proactive = 0,
    /// <summary>受到合法攻击后才反击，不因发现敌对目标主动交战。</summary>
    Retaliate = 1
}
