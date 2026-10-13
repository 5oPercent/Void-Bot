using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class ImportEraSkillDraftCommand : ICommand
{
    /// <summary>预期草稿修订号。</summary>
    public long ExpectedDraftRevision { get; set; }
    /// <summary>合并或完整替换草稿，均不发布。</summary>
    public EraSkillDraftImportMode Mode { get; set; }
    /// <summary>需要导入的完整合法配置。</summary>
    public SkillConfiguration Configuration { get; set; } = new();
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class ImportEraSkillDraftResponse : EraResponse<EraSkillDraftStatus>
{
}
