using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class RestoreEraDraftPublicationCommand : ICommand
{
    /// <summary>预期草稿修订号。</summary>
    public long ExpectedDraftRevision { get; set; }
    /// <summary>要恢复到草稿的历史版本；不直接回退发布版本。</summary>
    public long Version { get; set; }
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class RestoreEraDraftPublicationResponse : EraResponse<EraSkillDraftStatus>
{
}
