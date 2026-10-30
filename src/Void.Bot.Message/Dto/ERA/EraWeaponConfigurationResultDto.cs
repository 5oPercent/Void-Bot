namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器配置写入并发布成功后的结果，返回完整的新武器目录及共享发布版本。</summary>
public sealed class EraWeaponConfigurationResultDto
{
    /// <summary>本次保存对象的标识；操作未返回特定保存对象时为空。</summary>
    public Guid? SavedId { get; set; }
    /// <summary>成功发布后的整套装备目录版本，不是模板或装备实例的独立并发版本。</summary>
    public long Version { get; set; }
    /// <summary>本次发布后的完整武器目录，包含全部五个配置集合。</summary>
    public EraWeaponCatalogDto Configuration { get; set; } = new EraWeaponCatalogDto();
}
