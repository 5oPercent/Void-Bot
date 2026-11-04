using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>各业务定义表共享的编辑契约；不是共用存储表或发布快照。</summary>
public interface IEraDataDefinition : IEntity<Guid>
{
    EraDataKind Kind { get; }
    string StableId { get; set; }
    long Version { get; set; }
    string Payload { get; set; }
    Guid? LastModifiedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
