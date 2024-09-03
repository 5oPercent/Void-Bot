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
        
        var serilogSetting = new SerilogSetting(configuration);
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", serilogSetting.Application)
            .WriteTo.Console()
            .WriteTo.Seq(serilogSetting.Seq.ServerUrl, apiKey: serilogSetting.Seq.ApiKey)
            .CreateLogger();
        
        var connectionSetting = new ConnectionStringSetting(configuration);
        new DbUpRunner(connectionSetting.Value).Run(nameof(Core.Dbup));
        
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