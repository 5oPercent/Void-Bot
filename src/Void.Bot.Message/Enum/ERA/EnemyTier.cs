namespace Void.Bot.Message.Enum.ERA;

/// <summary>敌人等阶；选择本敌人倍率表的对应档位与客户端 HUD 样式，不切换 AI。</summary>
public enum EnemyTier
{
    /// <summary>普通 Minor：白血条，无标记。</summary>
    Minor = 0,
    /// <summary>精英 Elite：黄血条，无标记。</summary>
    Elite = 1,
    /// <summary>精锐 Major：黄血条，中心正三角顺时针旋转 25 度。</summary>
    Major = 2,
    /// <summary>首领 Ultra：橙色血条，中心骷髅。</summary>
    Ultra = 3,
    /// <summary>头目 Boss：遭遇期间持续显示屏幕顶部的大血条。</summary>
    Boss = 4
}
