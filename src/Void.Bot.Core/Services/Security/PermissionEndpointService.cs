using System.Net;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IPermissionEndpointService : IScope
{
    Task<GetEndpointsByPermissionIdResponse> GetEndpointsByPermissionIdAsync(
        GetEndpointsByPermissionIdRequest request, CancellationToken cancellationToken);
    
    Task<AssignEndpointsToPermissionResponse> AssignEndpointsToPermissionAsync(
        AssignEndpointsToPermissionCommand command, CancellationToken cancellationToken);
    
    Task<UpdatePermissionEndpointsResponse> UpdatePermissionEndpointsAsync(
        UpdatePermissionEndpointsCommand command, CancellationToken cancellationToken);
    
    Task<RemoveEndpointsFromPermissionResponse> RemoveEndpointsFromPermissionAsync(
        RemoveEndpointsFromPermissionCommand command, CancellationToken cancellationToken);
}

public class PermissionEndpointService : IPermissionEndpointService
{
    private readonly IMapper _mapper;
    private readonly IPermissionDataProvider _permissionDataProvider;
    private readonly IEndpointService _endpointService;
    private readonly IRepository<PermissionEndpoint> _permissionEndpointRep;
    
    public PermissionEndpointService(
        IMapper mapper,
        IPermissionDataProvider permissionDataProvider,
        IEndpointService endpointService,
        IRepository<PermissionEndpoint> permissionEndpointRep)
    {
        _mapper = mapper;
        _permissionDataProvider = permissionDataProvider;
        _endpointService = endpointService;
        _permissionEndpointRep = permissionEndpointRep;
    }

    public async Task<GetEndpointsByPermissionIdResponse> GetEndpointsByPermissionIdAsync(
        GetEndpointsByPermissionIdRequest request, CancellationToken cancellationToken)
    {
        var response = new GetEndpointsByPermissionIdResponse();
        
        var endpoints = await _permissionEndpointRep.QueryNoTracking()
            .Where(pe => pe.PermissionId == request.PermissionId)
            .Select(pe => pe.Endpoint)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        response.Data = GetExistEndpointDtos(endpoints);
        return response;
    }

    public async Task<AssignEndpointsToPermissionResponse> AssignEndpointsToPermissionAsync(
        AssignEndpointsToPermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new AssignEndpointsToPermissionResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = "Permission not found.";
            return response;
        }
        
        var endpoints = command.Endpoints.Distinct().ToList();
        var existingEndpointDtos = GetExistEndpointDtos(endpoints);
        var nonExistingEndpoints = endpoints.Where(e => existingEndpointDtos.All(ed => ed.Endpoint != e)).ToList();

        if (nonExistingEndpoints.Count != 0)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = $"Endpoints not found: {string.Join(", ", nonExistingEndpoints)}";
            return response;
        }
        
        var existingPermissionEndpoints = await _permissionEndpointRep.QueryNoTracking()
            .Where(pe => pe.PermissionId == command.PermissionId && endpoints.Contains(pe.Endpoint))
            .Select(pe => pe.Endpoint)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var newEndpoints = endpoints.Except(existingPermissionEndpoints).ToList();
        
        var newPermissionEndpoints = newEndpoints.Select(e => new PermissionEndpoint
        {
            PermissionId = command.PermissionId,
            Endpoint = e
        }).ToList();
        
        await _permissionEndpointRep.InsertManyAsync(newPermissionEndpoints, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<UpdatePermissionEndpointsResponse> UpdatePermissionEndpointsAsync(
        UpdatePermissionEndpointsCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdatePermissionEndpointsResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = "Permission not found.";
            return response;
        }
        
        await _permissionEndpointRep.DeleteAsync(pe => pe.PermissionId == command.PermissionId ,cancellationToken).ConfigureAwait(false);

        if (command.Endpoints.Count == 0)
        {
            return response;
        }
        
        var endpoints = command.Endpoints.Distinct().ToList();
        var existingEndpointDtos = GetExistEndpointDtos(endpoints);
        
        var newPermissionEndpoints = existingEndpointDtos.Select(e => new PermissionEndpoint
        {
            PermissionId = command.PermissionId,
            Endpoint = e.Endpoint
        }).ToList();
        
        await _permissionEndpointRep.InsertManyAsync(newPermissionEndpoints, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemoveEndpointsFromPermissionResponse> RemoveEndpointsFromPermissionAsync(
        RemoveEndpointsFromPermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new RemoveEndpointsFromPermissionResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = "Permission not found.";
            return response;
        }

        await _permissionEndpointRep.DeleteAsync(pe => pe.PermissionId == command.PermissionId && command.Endpoints.Contains(pe.Endpoint), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    private List<EndpointDto> GetExistEndpointDtos(List<string> endpoints)
    {
        return _endpointService.GetEndpoints().Where(x => endpoints.Contains(x.Endpoint)).ToList();
    }
}