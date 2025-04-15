using System.Net;
using Serilog;
using Void.Bot.Message.Commands.Sync;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto.Bungie;

namespace Void.Bot.Core.Services.Sync;

public partial interface ISyncService : IScope
{
    public Task<SynchronizeDestiny2ManifestResponse> SynchronizeDestiny2ManifestAsync(
        SynchronizeDestiny2ManifestCommand command, CancellationToken cancellationToken);
}

public partial class SyncService : ISyncService
{
    public async Task<SynchronizeDestiny2ManifestResponse> SynchronizeDestiny2ManifestAsync(
        SynchronizeDestiny2ManifestCommand command, CancellationToken cancellationToken)
    {
        var response = new SynchronizeDestiny2ManifestResponse();
        
        var result = await _bungieDataProvider.GetManifestAsync(cancellationToken);
        if (result == null)
        {
            response.Code = HttpStatusCode.NoContent;
            response.Message = "No manifest found.";
            return response;
        }
        
        var downloadFlag = false;
        var version = await _cacheManager.Get<string>("Destiny2:Manifest:Version", cancellationToken);
        
        if (command.ForceSync || string.IsNullOrEmpty(version) || result.Response.Version != version)
        {
            downloadFlag = true;
            await _cacheManager.Set("Destiny2:Manifest:Version", result.Response.Version, cancellationToken: cancellationToken);
        }

        if (downloadFlag)
        {
            response.Message = $"Version {result.Response.Version} found. Downloading...";
            _backgroundJob.Enqueue(() => SyncDestiny2ManifestToCache(
                result.Response.JsonWorldComponentContentPaths,
                command.Languages, command.Components, cancellationToken));
        }
        else
        {
            response.Message = $"Version {result.Response.Version} already exists. No need to download.";
        }

        return response;
    }

    public async Task SyncDestiny2ManifestToCache(
        BungieLanguageComponentContentDto componentContentPaths,
        List<string> languages, List<string>? components, CancellationToken cancellationToken)
    {
        for (var i = 0; i < languages.Count; i++)
        {
            languages[i] = languages[i].Replace("-", "_");
        }
        languages = languages.Distinct().ToList();
        
        // 针对每种语言获取并保存相应的组件数据
        foreach (var language in languages)
        {
            // 使用反射获取特定语言的组件内容
            var languageProperty = componentContentPaths.GetType().GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, language, StringComparison.OrdinalIgnoreCase));
            if (languageProperty == null)
            {
                Log.Warning("Language {Language} not found in manifest", language);
                continue;
            }
        
            var pathDto = languageProperty.GetValue(componentContentPaths) as BungieLanguageComponentContentPathDto;
            if (pathDto == null)
            {
                Log.Warning("Failed to get component content paths for language: {Language}", language);
                continue;
            }
        
            await SaveDestiny2ManifestToCache(pathDto, language, components, cancellationToken);
        }
    }

    private async Task SaveDestiny2ManifestToCache(
        BungieLanguageComponentContentPathDto pathDto,
        string language, List<string>? components, CancellationToken cancellationToken)
    {
        Log.Information("Start saving Destiny 2 manifest to cache for language: {Language}", language);
        
        // 获取所有需要下载的组件
        var properties = typeof(BungieLanguageComponentContentPathDto).GetProperties();
        if (components is { Count: > 0 })
        {
            properties = properties
                .Where(p => components.Any(c => string.Equals(p.Name, c, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        
            Log.Information("Filtering components. Total components to download: {Count}", properties.Length);
        }
        var successCount = 0;
        var failedCount = 0;
        
        foreach (var property in properties)
        {
            try
            {
                // 获取组件路由
                var path = property.GetValue(pathDto)?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var componentUrl = $"{BungieConstants.Domain}{path}";
                var componentName = property.Name;

                // 下载组件数据
                var jsonData = await DownloadDestiny2ManifestJsonDataAsync(componentUrl, cancellationToken);

                if (string.IsNullOrEmpty(jsonData))
                {
                    Log.Warning("Failed to download component: {Component} for language: {Language}", componentName, language);
                    failedCount++;
                    continue;
                }

                // 保存到缓存
                await _cacheManager.Set($"Destiny2:Manifest:{language}:{componentName}", jsonData, cancellationToken: cancellationToken);
                successCount++;

                Log.Information("Successfully cached component: {Component} for language: {Language}", componentName, language);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while cached component: {PropertyName} for language: {Language}", property.Name, language);
                failedCount++;
            }
        }

        Log.Information("Finished cached Destiny 2 manifest to cache for language: {Language}. Success: {SuccessCount}, Failed: {FailedCount}",
            language, successCount, failedCount);
    }
    
    private async Task<string> DownloadDestiny2ManifestJsonDataAsync(string url, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory.CreateClient().GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning("Download manifest file failed. Status code: {StatusCode}", response.StatusCode);
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}