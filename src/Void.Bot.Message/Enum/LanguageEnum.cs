using System.ComponentModel;

namespace Void.Bot.Message.Enum;

public enum LanguageEnum
{
    [Description("Default Language")]
    Default = 0,
    
    [Description("Simplified Chinese")]
    ZhChs = 1,
    
    [Description("Traditional Chinese")]
    ZhCht = 2,
    
    [Description("English")]
    En = 3
}