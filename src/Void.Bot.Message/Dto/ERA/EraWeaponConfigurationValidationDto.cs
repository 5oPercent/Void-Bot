namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWeaponConfigurationValidationDto
{
    public long Version { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
