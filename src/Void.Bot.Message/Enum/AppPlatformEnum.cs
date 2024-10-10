using System.ComponentModel;

namespace Void.Bot.Message.Enum;

public enum AppPlatformEnum
{
    [Description("Self")]
    Void = 0,
    
    [Description("Steam")]
    Steam = 1,
    
    [Description("Bungie")]
    Bungie = 2,
    
    [Description("Discord")]
    Discord = 4,
    
    [Description("Tencent")]
    Tencent = 8
}