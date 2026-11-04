using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Services.ERA.AbilityProfile;

public partial interface IEraAbilityProfileService : IScope
{
}

/// <summary>角色学习与配装业务；配置定义通过只读目录查询获取，事务由工作单元提交。</summary>
public sealed partial class EraAbilityProfileService(IEraAbilityProfileDataProvider dataProvider,
    IEraCharacterContext characterContext, ILocalizationService localization, IMapper mapper,
    IEraAbilityCatalogQueryService catalog) : IEraAbilityProfileService
{
    private readonly IEraAbilityProfileDataProvider _dataProvider = dataProvider;
    private readonly IEraCharacterContext _characterContext = characterContext;
    private readonly IMapper _mapper = mapper;
    private readonly IEraAbilityCatalogQueryService _catalog = catalog;

    private T Error<T>(HttpStatusCode status, string key, Expression<Func<EraResonancesContextTemplate, object>> text)
        where T : VoidBotResponse, new() => new() { Code = status, ErrorCode = key, Message = localization.GetValue(text) };
}
