using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>可复用的武器框架配置；定义表现驱动、能力组合和补充属性，具体数值与评分端点由模板保存。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponFrameworkDto
{
    /// <summary>框架唯一标识；默认生成新 Guid，模板通过此标识引用框架。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>框架显示名称，必填且最多 128 字符；不作为框架身份。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>所属武器分类，如自动步枪、脉冲步枪、弓、刀剑；可自定义且为 1～64 字符，默认未分类，仅用于展示与筛选，不决定驱动或能力组合。</summary>
    public string Category { get; set; } = "未分类";
    /// <summary>框架说明，最多 2048 字符，可为空字符串。</summary>
    public string Note { get; set; } = string.Empty;
    /// <summary>是否允许新增模板引用，默认启用；停用不移除现有引用，也不决定现有模板能否生成。</summary>
    public bool IsEnabled { get; set; } = true;
    /// <summary>可自定义的客户端表现流程键，必填且为 1～32 位 ASCII 字母、数字、点、下划线或短横线，区分大小写；不决定能力组合，与模板的具体表现资源键区分。</summary>
    public string PresentationDriverKey { get; set; } = string.Empty;
    /// <summary>启用的补充属性，不含六项公共属性；必须与能力组合要求完全一致且不得重复。</summary>
    public List<EraWeaponStat> SupplementaryStats { get; set; } = new();
    /// <summary>框架的类型化能力集合，最多 8 项且类型不得重复；命中方式及发射、蓄力的组合关系独立于表现流程键校验。</summary>
    public List<EraWeaponCapabilityDto> Capabilities { get; set; } = new();
}
