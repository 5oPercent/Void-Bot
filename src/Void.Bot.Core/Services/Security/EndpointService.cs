using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Core.Services.Security;

public interface IEndpointService : ISingleton
{
    public List<EndpointDto> GetEndpoints();
    
    public void UpdateEndpoints(List<EndpointDto> endpoints);
}

public class EndpointService : IEndpointService
{
    private List<EndpointDto> _endpoints = [];

    public List<EndpointDto> GetEndpoints()
    {
        return _endpoints;
    }

    public void UpdateEndpoints(List<EndpointDto> endpoints)
    {
        _endpoints = endpoints;
    }
}