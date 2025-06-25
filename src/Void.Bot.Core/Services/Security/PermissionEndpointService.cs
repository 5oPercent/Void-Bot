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
    
    Task<UnassignEndpointsFromPermissionResponse> UnassignEndpointsFromPermissionAsync(
        UnassignEndpointsFromPermissionCommand command, CancellationToken cancellationToken);
    
    Task<ReplacePermissionEndpointsResponse> ReplacePermissionEndpointsAsync(
        ReplacePermissionEndpointsCommand command, CancellationToken cancellationToken);
    
    Task<RemovePermissionsCascadeEndpointResponse> RemovePermissionsCascadeEndpointAsync(
        RemovePermissionsCascadeEndpointCommand command, CancellationToken cancellationToken);
    
    Task<RemoveEndpointsCascadePermissionResponse> RemoveEndpointsCascadePermissionAsync(
        RemoveEndpointsCascadePermissionCommand command, CancellationToken cancellationToken);
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
            .Where(x => x.PermissionId == request.PermissionId)
            .Select(x => x.Endpoint)
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
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Permission not found.";
            return response;
        }
        
        var endpoints = command.Endpoints.Distinct().ToList();
        var existingEndpointDtos = GetExistEndpointDtos(endpoints);
        var nonExistingEndpoints = endpoints.Where(e => existingEndpointDtos.All(ed => ed.Endpoint != e)).ToList();

        if (nonExistingEndpoints.Count != 0)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Endpoints not found: {string.Join(", ", nonExistingEndpoints)}";
            return response;
        }
        
        var existingPermissionEndpoints = await _permissionEndpointRep.QueryNoTracking()
            .Where(x => x.PermissionId == command.PermissionId && endpoints.Contains(x.Endpoint))
            .Select(x => x.Endpoint)
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

    public async Task<UnassignEndpointsFromPermissionResponse> UnassignEndpointsFromPermissionAsync(
        UnassignEndpointsFromPermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new UnassignEndpointsFromPermissionResponse();

        await _permissionEndpointRep.DeleteAsync(x => x.PermissionId == command.PermissionId && command.Endpoints.Contains(x.Endpoint), cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<ReplacePermissionEndpointsResponse> ReplacePermissionEndpointsAsync(
        ReplacePermissionEndpointsCommand command, CancellationToken cancellationToken)
    {
        var response = new ReplacePermissionEndpointsResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Permission not found.";
            return response;
        }
        
        await _permissionEndpointRep.DeleteAsync(x => x.PermissionId == command.PermissionId ,cancellationToken).ConfigureAwait(false);

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

    public async Task<RemovePermissionsCascadeEndpointResponse> RemovePermissionsCascadeEndpointAsync(
        RemovePermissionsCascadeEndpointCommand command, CancellationToken cancellationToken)
    {
        var response = new RemovePermissionsCascadeEndpointResponse();
        
        await _permissionEndpointRep.DeleteAsync(x => command.PermissionIds.Contains(x.PermissionId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemoveEndpointsCascadePermissionResponse> RemoveEndpointsCascadePermissionAsync(
        RemoveEndpointsCascadePermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new RemoveEndpointsCascadePermissionResponse();
        
        await _permissionEndpointRep.DeleteAsync(x => command.Endpoints.Contains(x.Endpoint), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    private List<EndpointDto> GetExistEndpointDtos(List<string> endpoints)
    {
        return _endpointService.GetEndpoints().Where(x => endpoints.Contains(x.Endpoint)).ToList();
    }
}