namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWeaponConfigurationResultDto
{
    public long Version { get; set; }
    public EraWeaponCatalogDto Configuration { get; set; } = new EraWeaponCatalogDto();
}
