using System.Net;
using System.Text.Json.Serialization;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Sync;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Request.Sync;

namespace Void.Bot.Core.Services.Sync;

public partial interface ISyncService : IScope
{
    public Task<GetDestiny2ManifestSyncOptionsResponse> GetDestiny2ManifestSyncOptionsAsync(
        CancellationToken cancellationToken);

    public Task<SynchronizeDestiny2ManifestResponse> SynchronizeDestiny2ManifestAsync(
        SynchronizeDestiny2ManifestCommand command, CancellationToken cancellationToken);
}

public partial class SyncService : ISyncService
{
    public Task<GetDestiny2ManifestSyncOptionsResponse> GetDestiny2ManifestSyncOptionsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new GetDestiny2ManifestSyncOptionsResponse
        {
            Data = new Destiny2ManifestSyncOptionsDto
            {
                Languages = _destiny2Setting.SyncLanguages.ToList(),
                Components = _destiny2Setting.SyncManifests.ToList()
            }
        });
    }

    public async Task<SynchronizeDestiny2ManifestResponse> SynchronizeDestiny2ManifestAsync(
        SynchronizeDestiny2ManifestCommand command, CancellationToken cancellationToken)
    {
        var response = new SynchronizeDestiny2ManifestResponse();
        
        var result = await _bungieDataProvider.GetDestiny2ManifestAsync(cancellationToken);
        if (result == null)
        {
            response.Code = HttpStatusCode.NoContent;
            response.Message = "No manifest found.";
            Log.Information(response.Message);
            return response;
        }
        
        var downloadFlag = false;
        var version = await _cacheManager.Get<string>(Destiny2ManifestConstants.VersionCacheKey, cancellationToken);
        
        if (command.ForceSync || string.IsNullOrEmpty(version) || result.Response.Version != version)
        {
            downloadFlag = true;
            await _cacheManager.Set(Destiny2ManifestConstants.VersionCacheKey, result.Response.Version, cancellationToken: cancellationToken);
        }

        if (downloadFlag)
        {
            response.Message = $"Version {result.Response.Version} found. Downloading...";
            _backgroundJob.Enqueue(() => CacheDestiny2ManifestAsync(
                result.Response.JsonWorldComponentContentPaths, command.Languages, command.Components, cancellationToken));
        }
        else
        {
            response.Message = $"Version {result.Response.Version} already exists.";
        }
        Log.Information(response.Message);

        return response;
    }

    public async Task CacheDestiny2ManifestAsync(
        Destiny2LanguageComponentContentDto languageComponentContentDto,
        List<string> languages, List<string>? components, CancellationToken cancellationToken)
    {
        for (var i = 0; i < languages.Count; i++)
        {
            languages[i] = LanguageExtension.MapI18nCodeToEnum(languages[i]).ToI18nCode();
        }
        languages = languages.Distinct().ToList();
        
        // 针对每种语言获取并保存相应的组件数据
        foreach (var language in languages)
        {
            // 使用反射获取特定语言的组件内容
            var languageProperty = languageComponentContentDto.GetType().GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(p.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                            .OfType<JsonPropertyNameAttribute>()
                            .FirstOrDefault()?.Name ?? p.Name,
                        language, StringComparison.OrdinalIgnoreCase));
            if (languageProperty == null)
            {
                Log.Warning("Language {Language} not found in manifest", language);
                continue;
            }
        
            var componentPathDto = languageProperty.GetValue(languageComponentContentDto) as Destiny2ManifestComponentContentPathDto;
            if (componentPathDto == null)
            {
                Log.Warning("Failed to get component content paths for language: {Language}", language);
                continue;
            }
        
            await CacheDestiny2ManifestComponentsAsync(componentPathDto, language, components, cancellationToken);
        }
    }

    private async Task CacheDestiny2ManifestComponentsAsync(
        Destiny2ManifestComponentContentPathDto componentPathDto,
        string language, List<string>? components, CancellationToken cancellationToken)
    {
        Log.Information("Start caching Destiny 2 manifest for language: {Language}", language);
        
        // 获取所有需要下载的组件
        var properties = typeof(Destiny2ManifestComponentContentPathDto).GetProperties();
        if (components is { Count: > 0 })
        {
            properties = properties
                .Where(p => components.Any(c => string.Equals(Destiny2ManifestConstants.ComponentPropertyNameMap[p], c, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }
        var successCount = 0;
        var failedComponents = new List<string>();
        
        foreach (var property in properties)
        {
            var componentName = property.Name.Replace("Destiny", string.Empty).Replace("Definition", string.Empty);
            
            try
            {
                // 获取组件路由
                var componentPath = property.GetValue(componentPathDto)?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(componentPath))
                {
                    continue;
                }
                
                var componentData = await DownloadDestiny2ManifestComponentJsonDataAsync($"{BungieConstants.BungieUrl}{componentPath}", cancellationToken);

                if (string.IsNullOrEmpty(componentData))
                {
                    Log.Warning("Failed to download component: {Component} for language: {Language}", componentName, language);
                    failedComponents.Add(componentName);
                    continue;
                }

                await _cacheManager.Set($"{Destiny2ManifestConstants.ComponentPrefixCacheKey}:{componentName}:{language}", componentData, cancellationToken: cancellationToken);
                successCount++;

                Log.Information("Successfully cached component: {Component} for language: {Language}", componentName, language);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while cached component: {Component} for language: {Language}", componentName, language);
                failedComponents.Add(componentName);
            }
        }

        if (failedComponents.Count == 0)
        {
            Log.Information("Finished cached Destiny 2 manifest to cache for language: {Language}. Success: {SuccessCount}",
                language, successCount);
        }
        else
        {
            Log.Information("Finished cached Destiny 2 manifest to cache for language: {Language}. Success: {SuccessCount}, Failed: {FailedComponents}",
                language, successCount, string.Join(", ", failedComponents));
        }
    }
    
    private async Task<string> DownloadDestiny2ManifestComponentJsonDataAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClientFactory.CreateClient().GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning("Download manifest file failed. Status code: {StatusCode}", response.StatusCode);
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
