namespace Void.Bot.Message.Dto.ERA;

public sealed class EraArmorConfigurationResultDto
{
    public Guid? SavedId { get; set; }
    public long Version { get; set; }
    public EraArmorCatalogDto Configuration { get; set; } = new EraArmorCatalogDto();
}
