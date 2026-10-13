using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>
/// 将版本化 JSON 存储实体转换为纯消息 DTO；存储版本覆盖载荷中的版本字段。
/// </summary>
public class EraSkillMapping : VoidBotMappingProfile
{
    public EraSkillMapping()
    {
        CreateMap<EraSkillConfiguration, SkillConfiguration>()
            .ConvertUsing(source => ConvertConfiguration(source));
        CreateMap<EraPlayerSkillProfile, PlayerSkillProfile>()
            .ConvertUsing(source => ConvertProfile(source));
    }

    private static SkillConfiguration ConvertConfiguration(EraSkillConfiguration source)
    {
        var configuration = JsonSerializer.Deserialize<SkillConfiguration>(source.Payload);
        if (configuration == null)
        {
            throw new InvalidOperationException("Skill configuration payload is empty.");
        }

        configuration.Version = source.Version;
        return configuration;
    }

    private static PlayerSkillProfile ConvertProfile(EraPlayerSkillProfile source)
    {
        var profile = JsonSerializer.Deserialize<PlayerSkillProfile>(source.Payload);
        if (profile == null)
        {
            throw new InvalidOperationException("Skill profile payload is empty.");
        }

        profile.Version = source.Version;
        return profile;
    }
}
