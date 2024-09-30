using System.ComponentModel;

namespace Void.Bot.Message.Enum;

public enum HangfireStorageTypeEnum
{
    [Description("Memory")]
    Memory = 0,
    
    [Description("Redis")]
    Redis = 1
}