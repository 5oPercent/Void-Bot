using System.Text.RegularExpressions;
using Void.Bot.Message.Constants.Language;

namespace Void.Bot.Message.Regexs;

public static partial  class LanguageRegexs
{
    [GeneratedRegex(LanguageConstants.YamlKeyPattern)]
    public static partial Regex YamlKey();
}