using Void.Bot.Api.Extension;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Settings.Language;

namespace Void.Bot.Api.HostedService;

public class InitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LanguageSetting _languageSetting;

    public InitializationHostedService(
        IServiceProvider serviceProvider,
        LanguageSetting languageSetting)
    {
        _serviceProvider = serviceProvider;
        _languageSetting = languageSetting;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ScanEndpointsAsync(cancellationToken).ConfigureAwait(false);
        
        if (_languageSetting.GenerateDefaultYamlFile)
        {
            await GenerateDefaultYamlFileAsync(cancellationToken).ConfigureAwait(false);
        }
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
        
        await languageService.GenerateDefaultYamlFileAsync(
            cancellationToken,
            _languageSetting.OverwriteDefaultYamlFile).ConfigureAwait(false);
    }
}