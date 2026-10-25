namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWeaponConfigurationResultDto
{
    public Guid? SavedId { get; set; }
    public long Version { get; set; }
    public EraWeaponCatalogDto Configuration { get; set; } = new EraWeaponCatalogDto();
}
