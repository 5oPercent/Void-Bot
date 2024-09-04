using Autofac;

namespace Void.Bot.IntegrationTest;

public class IntegrationUtilBase : IntegrationTestBase
{
    public IntegrationUtilBase(ILifetimeScope lifetimeScope)
    {
        SetupScope(lifetimeScope);
    }
}