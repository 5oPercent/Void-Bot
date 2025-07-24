using Autofac;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Api.HostedService;

public class InitializationHostedService : IHostedService
{
    private readonly ILifetimeScope _lifetimeScope;

    public InitializationHostedService(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scope = _lifetimeScope.BeginLifetimeScope();
        
        var languageService = scope.Resolve<ILanguageService>();
        await languageService.GenerateDefaultYamlFileAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}