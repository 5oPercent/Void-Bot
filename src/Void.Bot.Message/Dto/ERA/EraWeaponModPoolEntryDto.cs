using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器改装候选池中的一项，使用正整数权重进行不放回抽取。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModPoolEntryDto
{
    /// <summary>候选池引用的公共改装定义标识；必须存在且类型与方案槽位一致。</summary>
    public Guid ModDefinitionId { get; set; }
    /// <summary>抽取的相对权重，必须为正整数；同一池的权重总和不得超过 Int32.MaxValue。</summary>
    public int Weight { get; set; }
}
