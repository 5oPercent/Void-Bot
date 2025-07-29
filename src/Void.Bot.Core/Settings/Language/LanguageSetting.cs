using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.Language;

public class LanguageSetting : IConfigurationSetting
{
    public bool GenerateDefaultYamlFile { get; set; }
    
    public LanguageSetting(IConfiguration configuration)
    {
        GenerateDefaultYamlFile = configuration.GetValue<bool>("Language:GenerateDefaultYamlFile");
    }
}