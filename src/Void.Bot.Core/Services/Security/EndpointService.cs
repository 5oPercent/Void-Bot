using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Core.Services.Language;
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
    private List<EndpointDto> _endpoints = [];

    public EndpointService(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    public Task<GetEndpointListResponse> GetEndpointListAsync(GetEndpointListRequest request)
    {
        return Task.FromResult(new GetEndpointListResponse
        {
            Data = _endpoints.Select(endpoint => new EndpointDto
            {
                Controller = endpoint.Controller,
                Category = endpoint.Category,
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
            endpoint.Category,
            endpoint.DescriptionKey,
            LanguageEnum.Default);
        return string.IsNullOrEmpty(description) ? endpoint.Description : description;
    }

    public Task ReplaceEndpointListAsync(ReplaceEndpointListCommand command)
    {
        _endpoints = command.Endpoints;
        
        return Task.CompletedTask;
    }
}
