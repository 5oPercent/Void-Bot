using Autofac.Extensions.DependencyInjection;
using Serilog;
using Void.Bot.Core.Dbup;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
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
        
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureLogging(l => l.AddSerilog(Log.Logger))
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .Build()
            .Run();
    }
}