using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Language;

public class LanguageSetting(IConfiguration configuration) : IConfigurationSetting
{
    public bool GenerateDefaultYamlFile { get; set; } = configuration.GetValue<bool>("Language:GenerateDefaultYamlFile");
    public bool OverwriteDefaultYamlFile { get; set; } = configuration.GetValue<bool>("Language:OverwriteDefaultYamlFile");
}