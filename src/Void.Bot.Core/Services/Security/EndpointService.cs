using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IEndpointService : ISingleton
{
    public Task<GetEndpointListResponse> GetEndpointListAsync(GetEndpointListRequest request);
    
    public Task ReplaceEndpointListAsync(ReplaceEndpointListCommand command);
}

public class EndpointService : IEndpointService
{
    private List<EndpointDto> _endpoints = [];

    public Task<GetEndpointListResponse> GetEndpointListAsync(GetEndpointListRequest request)
    {
        return Task.FromResult(new GetEndpointListResponse
        {
            Data = _endpoints
        });
    }

    public Task ReplaceEndpointListAsync(ReplaceEndpointListCommand command)
    {
        _endpoints = command.Endpoints;
        
        return Task.CompletedTask;
    }
}