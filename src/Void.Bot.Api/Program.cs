using Serilog;
using StackExchange.Redis;
using Void.Bot.Api.Extension;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Dbup;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        configuration.Get(typeof(AppSetting));
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ReleaseVersion", configuration.GetValue<string>("ReleaseVersion"))
            .CreateLogger();
        
        var connectionSetting = new ConnectionStringSetting(configuration);
        new DbUpRunner(connectionSetting.Mysql).Run(nameof(Core.Dbup));

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<CacheSetting>(configuration.GetSection("CacheSetting")); 
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetValue<string>("CacheSetting:RedisConnection")));  
        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<ICacheManager, CacheManager>();

        WebApplication.CreateBuilder(args).ConfigureHost().AddServices()
            .Build().ConfigureApp()
            .Run();
    }
}