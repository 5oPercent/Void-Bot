using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

/// <summary>后端生成的单件结果；尚未持久化时不代表正式装备资产。</summary>
public sealed class EraArmorGenerationResult
{
    /// <summary>本次生成采用的护甲品阶。</summary>
    public required EraArmorGrade Grade { get; init; }

    /// <summary>与六维结果一起持久化的生成规则版本。</summary>
    public required int GenerationRuleVersion { get; init; }

    /// <summary>生成后的六维原值，不含运行时最终生效值。</summary>
    public required EraArmorAttributesDto Attributes { get; init; }
}
