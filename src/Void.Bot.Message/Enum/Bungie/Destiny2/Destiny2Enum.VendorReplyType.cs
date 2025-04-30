using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2商人回复类型枚举，定义与商人交互时可选的回复类型
/// </summary>
public enum Destiny2VendorReplyTypeEnum
{
    /// <summary>
    /// 接受商人提供的任务、交易或对话选项
    /// </summary>
    [Description("接受")]
    Accept = 0,

    /// <summary>
    /// 拒绝商人提供的任务、交易或对话选项
    /// </summary>
    [Description("拒绝")]
    Decline = 1,

    /// <summary>
    /// 完成已接受的任务或交易
    /// </summary>
    [Description("完成")]
    Complete = 2
}