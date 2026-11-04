using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraAdmin")]
public sealed class EraAdminEndpointContextTemplate : IContextTemplate
{
    public string GetEraWeaponFrameworks { get; set; } = "分页查询武器框架";
    public string GetEraWeaponFramework { get; set; } = "读取武器框架及模板引用";
    public string SaveEraWeaponFramework { get; set; } = "原子保存武器框架";
    public string DisableEraWeaponFramework { get; set; } = "停用武器框架的新引用";
    public string DeleteEraWeaponFramework { get; set; } = "删除未引用的武器框架";
    public string GetEraWeaponTemplates { get; set; } = "分页查询武器模板";
    public string GetEraWeaponTemplate { get; set; } = "读取武器模板与各槽候选数量";
    public string SaveEraWeaponTemplate { get; set; } = "原子保存武器模板与候选池";
    public string DisableEraWeaponTemplate { get; set; } = "停用武器模板的新生成";
    public string DeleteEraWeaponTemplate { get; set; } = "删除未引用的武器模板";
    public string GetEraWeaponModifications { get; set; } = "分页查询公共武器改装";
    public string GetEraWeaponModification { get; set; } = "读取武器改装及属性修正规则";
    public string SaveEraWeaponModification { get; set; } = "原子保存公共武器改装";
    public string DeleteEraWeaponModification { get; set; } = "删除未引用的公共武器改装";
    public string GetEraWeaponSlotConfigurations { get; set; } = "分页查询武器改装槽位方案";
    public string GetEraWeaponSlotTypes { get; set; } = "分页查询武器改装槽类型";
    public string GetEraWeaponSlotConfiguration { get; set; } = "读取武器改装槽位方案";
    public string GetEraWeaponSlotType { get; set; } = "读取武器改装槽类型";
    public string SaveEraWeaponSlotConfiguration { get; set; } = "原子保存武器改装槽位方案";
    public string SaveEraWeaponSlotType { get; set; } = "原子保存武器改装槽类型";
    public string DeleteEraWeaponSlotConfiguration { get; set; } = "删除未引用的武器改装槽位方案";
    public string DeleteEraWeaponSlotType { get; set; } = "删除未引用的武器改装槽类型";
    public string ValidateEraWeaponConfiguration { get; set; } = "校验待保存武器配置";
    public string SaveEraWeaponConfiguration { get; set; } = "保存完整武器配置";
    public string GetEraAdminPlayer { get; set; } = "官方查询指定 ERA 玩家的容量与版本";
    public string UpdateEraConfig { get; set; } = "保存 ERA 官方配置";
    public string SetEraWarehouseCapacity { get; set; } = "设置指定玩家的仓库容量";
    public string AdjustEraWarehouseCapacity { get; set; } = "增减指定玩家的仓库容量";
    public string GrantEraLearnedSkill { get; set; } = "管理员授予指定角色学习记录";
    public string GetEraSkillData { get; set; } = "读取当前配置状态";
    public string ExportEraSkillData { get; set; } = "读取当前配置";
    public string ListEraDataSystem { get; set; } = "分页读取晶格体系配置";
    public string GetEraDataSystem { get; set; } = "读取单个晶格体系配置";
    public string SaveEraDataSystem { get; set; } = "保存单个晶格体系配置";
    public string DeleteEraDataSystem { get; set; } = "删除单个晶格体系配置";
    public string ListEraDataSkill { get; set; } = "分页读取谐振技能配置";
    public string GetEraDataSkill { get; set; } = "读取单个谐振技能配置";
    public string SaveEraDataSkill { get; set; } = "保存单个谐振技能配置";
    public string DeleteEraDataSkill { get; set; } = "删除单个谐振技能配置";
    public string ListEraDataBuff { get; set; } = "分页读取BUFF配置";
    public string GetEraDataBuff { get; set; } = "读取单个BUFF配置";
    public string SaveEraDataBuff { get; set; } = "保存单个BUFF配置";
    public string DeleteEraDataBuff { get; set; } = "删除单个BUFF配置";
    public string ListEraDataEnemy { get; set; } = "分页查询敌人配置";
    public string GetEraDataEnemy { get; set; } = "读取敌人配置";
    public string SaveEraDataEnemy { get; set; } = "保存敌人配置";
    public string DeleteEraDataEnemy { get; set; } = "删除敌人配置";
    public string GetEraDataSettings { get; set; } = "读取通用池与禁用配置设置";
    public string SaveEraDataSettings { get; set; } = "保存通用池与禁用配置设置";
    public string GetEraDataReferences { get; set; } = "查询配置反向引用";
}
