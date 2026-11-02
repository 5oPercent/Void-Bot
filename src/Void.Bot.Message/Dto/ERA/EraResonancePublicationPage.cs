namespace Void.Bot.Message.Dto.ERA;

/// <summary>谐振技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraResonancePublicationPage
{
    /// <summary>历史记录总量。</summary>
    public int Total { get; set; }

    /// <summary>当前分页历史摘要。</summary>
    public List<EraResonancePublicationInfo> Items { get; set; } = new();
}
