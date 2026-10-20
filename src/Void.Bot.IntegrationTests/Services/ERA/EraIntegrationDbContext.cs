using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Void.Bot.Core.Data;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.IntegrationTest.Services.ERA;

/// <summary>保持生产 MySQL 配置、映射和审计，仅给当前测试作用域附加 SQL 拦截器。</summary>
internal sealed class EraIntegrationDbContext : VoidBotDbContext
{
    private readonly DbCommandInterceptor _interceptor;

    public EraIntegrationDbContext(
        ConnectionStringSetting setting, ICurrentUser currentUser, DbCommandInterceptor interceptor)
        : base(setting, currentUser)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
