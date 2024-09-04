namespace Void.Bot.Core.Settings;

public interface IConfigurationSetting { }

public interface IConfigurationSetting<T> : IConfigurationSetting
{
    T Value { get; set; }
}