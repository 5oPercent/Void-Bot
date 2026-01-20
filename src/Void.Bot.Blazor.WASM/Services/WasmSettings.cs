namespace Void.Bot.Blazor.WASM.Services;

public class WasmSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;

    public WasmSettings(IConfiguration configuration)
    {
        configuration.Bind("ApiBaseUrl", ApiBaseUrl);
    }
}
