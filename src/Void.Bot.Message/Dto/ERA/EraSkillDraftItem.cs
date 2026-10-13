namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftItem<T>
{
    /// <summary>稳定 ID。</summary>
    public string Id { get; set; } = "";

    /// <summary>单项版本。</summary>
    public long Version { get; set; }

    /// <summary>返回时的草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>草稿删除标记。</summary>
    public bool IsDeleted { get; set; }

    /// <summary>是否曾发布，用于保留稳定 ID 身份。</summary>
    public bool WasPublished { get; set; }

    /// <summary>独立对象定义。</summary>
    public T Definition { get; set; } = default!;
}
