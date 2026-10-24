using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraAdmin")]
public sealed class EraAdminEndpointContextTemplate : IContextTemplate
{
    public string GetEraWeaponTemplates { get; set; } = "分页查询武器模板";
    public string GetEraWeaponTemplate { get; set; } = "读取武器模板与各槽候选数量";
    public string SaveEraWeaponTemplate { get; set; } = "原子保存并发布武器模板与候选池";
    public string DisableEraWeaponTemplate { get; set; } = "停用武器模板的新生成";
    public string DeleteEraWeaponTemplate { get; set; } = "删除未引用的武器模板";
    public string GetEraWeaponModifications { get; set; } = "分页查询公共武器改装";
    public string GetEraWeaponModification { get; set; } = "读取武器改装及属性修正规则";
    public string SaveEraWeaponModification { get; set; } = "原子保存并发布公共武器改装";
    public string DeleteEraWeaponModification { get; set; } = "删除未引用的公共武器改装";
    public string ValidateEraWeaponConfiguration { get; set; } = "校验拟发布武器配置";
    public string PublishEraWeaponConfiguration { get; set; } = "原子发布完整武器配置";
    public string GetEraAdminPlayer { get; set; } = "官方查询指定 ERA 玩家的容量与版本";
    public string UpdateEraConfig { get; set; } = "发布 ERA 官方配置";
    public string SetEraWarehouseCapacity { get; set; } = "设置指定玩家的仓库容量";
    public string AdjustEraWarehouseCapacity { get; set; } = "增减指定玩家的仓库容量";
    public string GrantEraLearnedSkill { get; set; } = "管理员授予指定角色学习记录";
    public string GetEraSkillDraft { get; set; } = "读取草稿版本与待发布状态";
    public string InitializeEraSkillDraft { get; set; } = "从当前发布配置幂等初始化共享草稿";
    public string ExportEraSkillDraft { get; set; } = "导出已保存草稿，不发布";
    public string ListEraDraftSystem { get; set; } = "分页读取体系草稿";
    public string GetEraDraftSystem { get; set; } = "读取单个体系草稿";
    public string SaveEraDraftSystem { get; set; } = "保存单个体系草稿";
    public string DeleteEraDraftSystem { get; set; } = "标记删除单个体系草稿";
    public string RestoreEraDraftSystem { get; set; } = "恢复单个体系草稿";
    public string ListEraDraftSkill { get; set; } = "分页读取技能草稿";
    public string GetEraDraftSkill { get; set; } = "读取单个技能草稿";
    public string SaveEraDraftSkill { get; set; } = "保存单个技能草稿";
    public string DeleteEraDraftSkill { get; set; } = "标记删除单个技能草稿";
    public string RestoreEraDraftSkill { get; set; } = "恢复单个技能草稿";
    public string ListEraDraftBuff { get; set; } = "分页读取BUFF草稿";
    public string GetEraDraftBuff { get; set; } = "读取单个BUFF草稿";
    public string SaveEraDraftBuff { get; set; } = "保存单个BUFF草稿";
    public string DeleteEraDraftBuff { get; set; } = "标记删除单个BUFF草稿";
    public string RestoreEraDraftBuff { get; set; } = "恢复单个BUFF草稿";
    public string GetEraDraftSettings { get; set; } = "读取通用池与禁用草稿设置";
    public string SaveEraDraftSettings { get; set; } = "保存通用池与禁用草稿设置";
    public string ValidateEraSkillDraft { get; set; } = "校验草稿完整引用与发布预算";
    public string PreviewEraSkillDraft { get; set; } = "预览草稿差异与冻结版本";
    public string GetEraDraftReferences { get; set; } = "查询草稿反向引用";
    public string ImportEraSkillDraft { get; set; } = "合并或替换导入草稿，不发布";
    public string PublishEraSkillDraft { get; set; } = "原子发布已保存草稿";
    public string ListEraSkillPublications { get; set; } = "分页读取发布历史";
    public string GetEraSkillPublication { get; set; } = "读取指定历史配置";
    public string RestoreEraDraftPublication { get; set; } = "历史版本恢复至草稿，不直接发布";
}
