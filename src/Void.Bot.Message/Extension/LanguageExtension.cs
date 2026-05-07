using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Extension;

public static class LanguageExtension
{
    public static string ToLanguageCode(this LanguageEnum language)
    {
        return language switch
        {
            LanguageEnum.ZhChs => "zh-chs",
            LanguageEnum.ZhCht => "zh-cht",
            LanguageEnum.En => "en",
            _ => LanguageEnum.Default.ToString()
        };
    }
}
