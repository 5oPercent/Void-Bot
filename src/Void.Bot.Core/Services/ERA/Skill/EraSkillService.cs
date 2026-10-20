using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial interface IEraSkillService : IScope
{
}

/// <summary>
/// ERA 技能配置与角色配装的业务入口；事务由 Mediator 工作单元统一提交。
/// </summary>
public partial class EraSkillService : IEraSkillService
{
    private readonly IEraSkillDataProvider _dataProvider;
    private readonly IEraCharacterContext _characterContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public EraSkillService(
        IEraSkillDataProvider dataProvider,
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
        Expression<Func<EraSkillsContextTemplate, object>> text)
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
