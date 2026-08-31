using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IEndpointService : ISingleton
{
    public Task<GetEndpointListResponse> GetEndpointListAsync(GetEndpointListRequest request);
    
    public Task ReplaceEndpointListAsync(ReplaceEndpointListCommand command);
}

public class EndpointService : IEndpointService
{
    private readonly ILanguageService _languageService;
    private readonly ICurrentUser _currentUser;
    private List<EndpointDto> _endpoints = [];

    public EndpointService(ILanguageService languageService, ICurrentUser currentUser)
    {
        _languageService = languageService;
        _currentUser = currentUser;
    }

    public Task<GetEndpointListResponse> GetEndpointListAsync(GetEndpointListRequest request)
    {
        return Task.FromResult(new GetEndpointListResponse
        {
            Data = _endpoints.Select(endpoint => new EndpointDto
            {
                Controller = endpoint.Controller,
                Endpoint = endpoint.Endpoint,
                Method = endpoint.Method,
                DescriptionKey = endpoint.DescriptionKey,
                Description = ResolveDescription(endpoint)
            }).ToList()
        });
    }

    private string ResolveDescription(EndpointDto endpoint)
    {
        var description = _languageService.GetEndpointDescription(
            endpoint.Controller,
            endpoint.DescriptionKey,
            _currentUser.Language);
        return string.IsNullOrEmpty(description) ? endpoint.Description : description;
    }

    public Task ReplaceEndpointListAsync(ReplaceEndpointListCommand command)
    {
        _endpoints = command.Endpoints;
        
        return Task.CompletedTask;
    }
}
