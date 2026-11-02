using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService : IScope
{
}

/// <summary>
/// ERA 谐振技能配置与角色配装的业务入口；事务由 Mediator 工作单元统一提交。
/// </summary>
public partial class EraResonanceService : IEraResonanceService
{
    private readonly IEraResonanceDataProvider _dataProvider;
    private readonly IEraCharacterContext _characterContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public EraResonanceService(
        IEraResonanceDataProvider dataProvider,
        IEraCharacterContext characterContext,
        ICurrentUser currentUser,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _dataProvider = dataProvider;
        _characterContext = characterContext;
        _currentUser = currentUser;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    private T Error<T>(
        HttpStatusCode status,
        string key,
        Expression<Func<EraResonancesContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T()
        {
            Code = status,
            ErrorCode = key,
            Message = _localizationService.GetValue(text)
        };
    }

}
