using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>
/// 将角色配装 JSON 转换为消息 DTO；版本仅用于玩家配装并发校验。
/// </summary>
public class EraResonanceMapping : VoidBotMappingProfile
{
    public EraResonanceMapping()
    {
        CreateMap<EraCharacterResonanceProfile, CharacterResonanceProfile>()
            .ConvertUsing(source => ConvertProfile(source));
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
