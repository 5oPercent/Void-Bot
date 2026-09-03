using System.Linq.Expressions;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Language;

public interface ILocalizationService : IScope
{
    string GetValue<T>(
        Expression<Func<T, object>> expression,
        LanguageEnum language = LanguageEnum.Default,
        params object[] args)
        where T : IContextTemplate;

    string GetValue<T>(
        Expression<Func<T, object>> expression,
        string? languageCode,
        params object[] args)
        where T : IContextTemplate;

    string GetEndpointDescription(
        string category,
        string key,
        LanguageEnum language = LanguageEnum.Default);

    Dictionary<string, string> GetValues<T>(
        Expression<Func<T, object>> expression,
        params object[] args)
        where T : IContextTemplate;

}

public sealed class LocalizationService(
    ILanguageService languageService,
    ICurrentUser currentUser) : ILocalizationService
{
    public string GetValue<T>(
        Expression<Func<T, object>> expression,
        LanguageEnum language = LanguageEnum.Default,
        params object[] args)
        where T : IContextTemplate
    {
        return GetCore(expression, language, args);
    }

    public string GetValue<T>(
        Expression<Func<T, object>> expression,
        string? languageCode,
        params object[] args)
        where T : IContextTemplate
    {
        return GetCore(
            expression,
            LanguageExtension.MapI18nCodeToEnum(languageCode),
            args);
    }

    private string GetCore<T>(
        Expression<Func<T, object>> expression,
        LanguageEnum language,
        object[] args)
        where T : IContextTemplate
    {
        var targetLanguage = language == LanguageEnum.Default
            ? currentUser.Language
            : language;
        var value = languageService.GetContext(expression, targetLanguage);

        return args.Length == 0 ? value : string.Format(value, args);
    }

    public string GetEndpointDescription(
        string category,
        string key,
        LanguageEnum language = LanguageEnum.Default)
    {
        return languageService.GetEndpointDescription(category, key, language == LanguageEnum.Default ? currentUser.Language : language);
    }

    public Dictionary<string, string> GetValues<T>(
        Expression<Func<T, object>> expression,
        params object[] args)
        where T : IContextTemplate
    {
        return new Dictionary<string, string>
        {
            ["zh-CN"] = GetValue(expression, LanguageEnum.ZhChs, args),
            ["zh-TW"] = GetValue(expression, LanguageEnum.ZhCht, args),
            ["en-US"] = GetValue(expression, LanguageEnum.En, args)
        };
    }

}
