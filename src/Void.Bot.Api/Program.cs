using Serilog;
using Void.Bot.Api.Extension;
using Void.Bot.Core.Dbup;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        
        configuration.Get(typeof(AppSetting));
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ReleaseVersion", configuration.GetValue<string>("ReleaseVersion"))
            .CreateLogger();
        
        var connectionSetting = new ConnectionStringSetting(configuration);
        new DbUpRunner(connectionSetting.Mysql).Run(nameof(Core.Dbup));
        
        builder.ConfigureHost().AddServices()
            .Build().ConfigureApp()
            .Run();
    }
}
