namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器配置预检结果；预检不保存配置，也不增加发布版本。</summary>
public sealed class EraWeaponConfigurationValidationDto
{
    /// <summary>执行预检时读取的当前装备目录发布版本。</summary>
    public long Version { get; set; }
    /// <summary>预检错误列表；为空才表示配置通过当前校验。</summary>
    public List<string> Errors { get; set; } = new List<string>();
}
