using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

/// <summary>单条 SQL 读取的世界版本与成员集合。</summary>
public sealed record EraWorldReadSnapshot(EraWorld World, IReadOnlyList<EraWorldMember> Members);
