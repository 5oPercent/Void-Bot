using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>槽位方案中的一个改装槽位，定义生成时使用的候选池与候选数量。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponConfigurationSlotDto
{
    /// <summary>方案槽位唯一标识，默认生成新 Guid；生成实例时保留为实例槽位标识。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>槽位显示名称，必填且最多 128 字符；生成后保存在实例槽位快照中。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>槽位类型标识；候选池中的所有改装定义都必须属于此类型。</summary>
    public Guid SlotTypeId { get; set; }
    /// <summary>每件新实例在此槽生成的不同候选数量，范围 1～16，且不得超过池中不同改装的数量。</summary>
    public int CandidateCount { get; set; }
    /// <summary>槽位显示和生成顺序，必须为非负整数。</summary>
    public int DisplayOrder { get; set; }
    /// <summary>加权抽取候选池，最多 256 项；改装定义不得重复，生成候选后不会随池变化重抽。</summary>
    public List<EraWeaponModPoolEntryDto> Pool { get; set; } = new();
}
