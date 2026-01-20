using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Void.Bot.Blazor.WASM;
using Void.Bot.Blazor.WASM.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ThemeService>();

var app = builder.Build();
var authService = app.Services.GetRequiredService<AuthService>();
await authService.InitializeAsync();
var themeService = app.Services.GetRequiredService<ThemeService>();
await themeService.InitializeAsync();
await app.RunAsync();