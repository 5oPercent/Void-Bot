using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>
/// 将版本化 JSON 存储实体转换为纯消息 DTO；存储版本覆盖载荷中的版本字段。
/// </summary>
public class EraResonanceMapping : VoidBotMappingProfile
{
    public EraResonanceMapping()
    {
        CreateMap<EraResonanceConfiguration, ResonanceConfiguration>()
            .ConvertUsing(source => ConvertConfiguration(source));
        CreateMap<EraCharacterResonanceProfile, CharacterResonanceProfile>()
            .ConvertUsing(source => ConvertProfile(source));
    }

    private static ResonanceConfiguration ConvertConfiguration(EraResonanceConfiguration source)
    {
        var configuration = JsonSerializer.Deserialize<ResonanceConfiguration>(source.Payload);
        if (configuration == null)
        {
            throw new InvalidOperationException("Resonance configuration payload is empty.");
        }

        configuration.Version = source.Version;
        return configuration;
    }

    private static CharacterResonanceProfile ConvertProfile(EraCharacterResonanceProfile source)
    {
        var profile = JsonSerializer.Deserialize<CharacterResonanceProfile>(source.Payload);
        if (profile == null)
        {
            throw new InvalidOperationException("Resonance profile payload is empty.");
        }

        profile.Version = source.Version;
        return profile;
    }
}
