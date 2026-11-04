using System.Linq.Expressions;
using System.Net;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.ERA.Configuration.Models;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Services.ERA.Configuration;

/// <summary>复用配置错误响应、预算和分页规则，不提供业务写入入口。</summary>
public abstract class EraConfigurationServiceBase(ILocalizationService localization)
{
    protected T Error<T>(
        HttpStatusCode status,
        string key,
        Expression<Func<EraResonancesContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T()
        {
            Code = status,
            ErrorCode = key,
            Message = localization.GetValue(text)
        };
    }

    protected TResponse DataUnavailable<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.ServiceUnavailable, "era.data_unavailable", text => text.DataUnavailable);
    }

    protected TResponse DataInvalid<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.BadRequest, "era.data_invalid", text => text.InvalidData);
    }

    protected TResponse DataConflict<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.Conflict, "era.data_version_conflict", text => text.VersionConflict);
    }

    protected static bool HasDataBudget(EraDataView view, string payload, string? replacing = null)
    {
        return System.Text.Encoding.UTF8.GetByteCount(payload) + view.Records.Sum(r => (long)System.Text.Encoding.UTF8.GetByteCount(r.Payload)) + System.Text.Encoding.UTF8.GetByteCount(view.Workspace.SettingsPayload) - (replacing == null ? 0 : System.Text.Encoding.UTF8.GetByteCount(replacing)) <= 1048576;
    }

    protected static void ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take is < 1 or > 100)
        {
            throw new ArgumentException("分页范围无效。");
        }
    }

}
