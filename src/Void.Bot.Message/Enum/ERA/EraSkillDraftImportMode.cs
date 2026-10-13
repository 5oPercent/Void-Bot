namespace Void.Bot.Message.Enum.ERA;

/// <summary>导入只修改草稿，不直接发布。</summary>
public enum EraSkillDraftImportMode
{
    /// <summary>按类型和 ID 合并定义，通用池与禁用列表有序去重合并。</summary>
    Merge = 0,
    /// <summary>用完整配置替换草稿，未列出的现有对象标记删除。</summary>
    Replace = 1
}
