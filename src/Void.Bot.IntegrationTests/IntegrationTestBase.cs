using System.Text.Json.Nodes;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NSubstitute;
using Respawn;
using Serilog;
using Void.Bot.Core;
using Void.Bot.Core.Data;
using Void.Bot.Core.Dbup;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.IntegrationTest;

public class IntegrationTestBase : IAsyncLifetime
{
    private const string Topic = "integration_test";
    
    public readonly ILifetimeScope LifetimeScope;
    
    private IConfiguration Configuration => LifetimeScope.Resolve<IConfiguration>();
    
    private Respawner _respawner = null!;

    public IntegrationTestBase()
    {
        var logger = Substitute.For<ILogger>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(l =>
        {
            l.AddSerilog();
        });
        
        var containerBuilder = new ContainerBuilder();
        
        var configuration = RegisterConfiguration(containerBuilder);
        
        containerBuilder.Populate(serviceCollection);
        containerBuilder.RegisterModule(new VoidBotModule(new IntegrationTestUser(), logger, configuration, typeof(IntegrationFixture).Assembly));
        containerBuilder.RegisterInstance(new MemoryCache(new MemoryCacheOptions())).AsSelf().As<IMemoryCache>().SingleInstance();
        
        LifetimeScope = containerBuilder.Build();
        
        new DbUpRunner(new ConnectionStringSetting(Configuration).Mysql).Run("Dbup");
    }
    
    private IConfiguration RegisterConfiguration(ContainerBuilder containerBuilder)
    {
        var targetJson = $"appsetting_{Topic}.json";
        File.Copy("appsettings.json", targetJson, true);

        var document = JsonNode.Parse(File.ReadAllText(targetJson)) ?? throw new InvalidOperationException();
        document["ConnectionStrings"]!["Mysql"] = document["ConnectionStrings"]!["Mysql"]!.ToString()
            .Replace("database=db_void_bot", $"database=db_void_bot_{Topic}");
        
        File.WriteAllText(targetJson, document.ToJsonString());
        
        var configuration = new ConfigurationBuilder().AddJsonFile(targetJson).Build();
        containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();
        
        configuration.Get(typeof(AppSetting));
        
        return configuration;
    }
    
    public async Task ResetDatabaseAsync()
    {
        await using var conn = new MySqlConnection(new ConnectionStringSetting(Configuration).Mysql);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public async Task InitializeAsync()
    {
        await using var conn = new MySqlConnection(new ConnectionStringSetting(Configuration).Mysql);
        await conn.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            TablesToIgnore =
            [
                "schemaversions"
            ],
            SchemasToInclude =
            [
                $"db_void_bot_{Topic}"
            ],
            DbAdapter = DbAdapter.MySql
        });
    }

    public Task DisposeAsync()
    {
        var context = LifetimeScope.Resolve<VoidBotDbContext>();
        return context.Database.EnsureDeletedAsync();
    }
}