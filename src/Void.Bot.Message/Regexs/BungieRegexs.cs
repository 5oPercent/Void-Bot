using System.Text.RegularExpressions;
using Void.Bot.Message.Constants.Bungie;

namespace Void.Bot.Message.Regexs;

public static partial class BungieRegexs
{
    [GeneratedRegex(BungieConstants.IdPattern)]
    public static partial Regex UniqueName();
    
    [GeneratedRegex("[-_]")]
    public static partial Regex Language();
}