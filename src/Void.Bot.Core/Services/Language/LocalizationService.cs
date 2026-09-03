using System.Linq.Expressions;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Language;

public interface ILocalizationService : IScope
{
    string Get<T>(
        Expression<Func<T, object>> expression,
        LanguageEnum language = LanguageEnum.Default,
        params object[] args)
        where T : IContextTemplate;
}

public sealed class LocalizationService(
    ILanguageService languageService,
    ICurrentUser currentUser) : ILocalizationService
{
    public string Get<T>(
        Expression<Func<T, object>> expression,
        LanguageEnum language = LanguageEnum.Default,
        params object[] args)
        where T : IContextTemplate
    {
        var targetLanguage = language == LanguageEnum.Default
            ? currentUser.Language
            : language;
        var value = languageService.GetContext(expression, targetLanguage);

        return args.Length == 0 ? value : string.Format(value, args);
    }
}
