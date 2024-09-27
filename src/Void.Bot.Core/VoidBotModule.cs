using System.Net;
using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Discord;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Mediator.Net;
using Mediator.Net.Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using Void.Bot.Core.Data;
using Void.Bot.Core.Middlewares.Logger;
using Void.Bot.Core.Middlewares.MessageLogger;
using Void.Bot.Core.Middlewares.UnifyResponse;
using Void.Bot.Core.Middlewares.UnitofWork;
using Void.Bot.Core.Services;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.Discord;
using Module = Autofac.Module;

namespace Void.Bot.Core;

public class VoidBotModule(ILogger logger, IConfiguration configuration, params Assembly[] assemblies) : Module
{
    private readonly Assembly[] _assemblies = assemblies.Length == 0
        ? new Assembly[] { typeof(VoidBotModule).Assembly }
        : assemblies.Concat(new[] { typeof(VoidBotModule).Assembly }).Distinct().ToArray();
    
    protected override void Load(ContainerBuilder builder)
    {
        RegisterLogger(builder);
        
        RegisterMediator(builder);

        RegisterAutoMapper(builder);

        RegisterDependency(builder);
        
        RegisterCaching(builder);
        
        RegisterSettings(builder);

        RegisterDbContext(builder);

        RegisterDiscordBotClient(builder);
    }
    
    // 注册日志
    private void RegisterLogger(ContainerBuilder builder)
    {
        builder.RegisterInstance(logger).AsSelf().AsImplementedInterfaces().SingleInstance();
    }
    
    // 注册中介者，用于解耦，后续调用 Service 请使用 IMediator
    private void RegisterMediator(ContainerBuilder builder)
    {
        var mediatorBuilder = new MediatorBuilder();

        mediatorBuilder.RegisterHandlers(_assemblies);
        mediatorBuilder.ConfigureGlobalReceivePipe(p =>
        {
            p.UseLogger();          // 统一日志记录
            p.UseMessageLogging();  // 特殊标记日志记录
            p.UseUnitOfWork();      // 事务处理
            p.UseUnifyResponse();   // 统一响应
        });

        builder.RegisterMediator(mediatorBuilder);
    }
    
    // 注册 AutoMapper，用于对象映射
    private void RegisterAutoMapper(ContainerBuilder builder)
    {
        builder.RegisterAutoMapper(false, _assemblies);
    }
    
    // 注册依赖注入
    private void RegisterDependency(ContainerBuilder builder)
    {
        var allServiceTypes = _assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && t.GetInterfaces().Any(i => i == typeof(IService)))
            .ToList();

        foreach (var type in allServiceTypes)
        {
            if (typeof(IScope).IsAssignableFrom(type))
                builder.RegisterType(type).AsImplementedInterfaces().InstancePerLifetimeScope();
            else if (typeof(ISingleton).IsAssignableFrom(type))
                builder.RegisterType(type).AsImplementedInterfaces().SingleInstance();
            else
                builder.RegisterType(type).AsImplementedInterfaces();
        }
    }
    
    // 注册缓存
    private void RegisterCaching(ContainerBuilder builder)
    {
        builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
    }
    
    // 注册配置
    private void RegisterSettings(ContainerBuilder builder)
    {
        var settingTypes = _assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && typeof(IConfigurationSetting).IsAssignableFrom(t))
            .ToArray();

        builder.RegisterTypes(settingTypes).AsSelf().SingleInstance();
    }
    
    // 注册数据库上下文
    private void RegisterDbContext(ContainerBuilder builder)
    {
        builder.RegisterType<VoidBotDbContext>()
            .AsSelf()
            .As<DbContext>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<Repository>().As<IRepository>().InstancePerLifetimeScope();
    }
    
    // 注册 Discord Bot 客户端
    public void RegisterDiscordBotClient(ContainerBuilder builder)
    {
        var clientConfig = new DiscordSocketConfig();
        
        var discordSetting = new DiscordSetting(configuration);
        if (discordSetting.EnableProxy)
        {
            clientConfig.WebSocketProvider = DefaultWebSocketProvider.Create(new WebProxy(AppSetting.ProxyUrl));
            clientConfig.RestClientProvider = DefaultRestClientProvider.Create(useProxy: true);
        }
        
        var client = new DiscordSocketClient(clientConfig);

        #region Setting Logger
        
        client.Log += log =>
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(log.Exception, log.Message);
                    break;
                case LogSeverity.Error:
                    logger.Error(log.Exception, log.Message);
                    break;
                case LogSeverity.Warning:
                    logger.Warning(log.Exception, log.Message);
                    break;
                case LogSeverity.Info:
                    logger.Information(log.Exception, log.Message);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    logger.Debug(log.Exception, log.Message);
                    break;
                default:
                    logger.Warning(log.Exception, "漏网之鱼: " + log.Message);
                    break;
            }

            return Task.CompletedTask;
        };

        #endregion

        builder.RegisterInstance(client).AsSelf().SingleInstance();
    }
}