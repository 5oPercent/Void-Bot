using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>目录契约校验；护甲和武器配置从各自关系表读取并校验。</summary>
public static class EraEquipmentConfigurationValidator
{
    public static void Validate(EraEquipmentManifestDto configuration)
    {
        if (configuration == null || configuration.SchemaVersion != 2 || configuration.Version < 1)
            throw new ArgumentException("Equipment configuration schema or version is invalid.");
        EraArmorConfigurationValidator.Validate(configuration.Armors);
    }
}
