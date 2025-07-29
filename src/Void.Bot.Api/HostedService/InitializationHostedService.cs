using Void.Bot.Api.Extension;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Api.HostedService;

public class InitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public InitializationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ScanEndpointsAsync(cancellationToken).ConfigureAwait(false);
        await GenerateDefaultYamlFileAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ScanEndpointsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var endpointService = scope.ServiceProvider.GetRequiredService<IEndpointService>();

        var endpoints = ScannerExtensions.ScanEndpoints();

        await endpointService.ReplaceEndpointListAsync(new ReplaceEndpointListCommand
        {
            Endpoints = endpoints
        });
    }

    private async Task GenerateDefaultYamlFileAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();
        
        await languageService.GenerateDefaultYamlFileAsync(cancellationToken).ConfigureAwait(false);
    }
}