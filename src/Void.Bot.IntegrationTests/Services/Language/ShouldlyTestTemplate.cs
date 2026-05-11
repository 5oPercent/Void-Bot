using Void.Bot.Core.Resources.i18n;
using Void.Bot.Message.Attributes;

namespace Void.Bot.IntegrationTest.Services.Language;

[Language("Test", "ShouldlySample")] 
public class ShouldlyTestTemplate : IContextTemplate
{
    public string Greeting { get; set; } = "Hello Shouldly";
}