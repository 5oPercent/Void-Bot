using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>技能草稿验证、导入、发布或历史消息。</summary>
public sealed class PublishEraSkillDraftCommand : ICommand
{
    /// <summary>预期草稿修订号。</summary>
    public long ExpectedDraftRevision { get; set; }
    /// <summary>预期当前发布版本。</summary>
    public long ExpectedPublishedVersion { get; set; }
    /// <summary>稳定发布请求 ID，用于持久化幂等。</summary>
    public Guid RequestId { get; set; }
}

/// <summary>操作结果，发布返回完整已发布配置。</summary>
public sealed class PublishEraSkillDraftResponse : EraResponse<SkillConfiguration>
{
}
