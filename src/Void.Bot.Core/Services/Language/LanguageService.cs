using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Language;

public interface ILanguageService : ISingleton
{
    public Task GenerateDefaultYamlFileAsync(CancellationToken cancellationToken);

    public string GetContext<T>(Expression<Func<T, object>> expression, LanguageEnum language = LanguageEnum.Default)
        where T : IContextTemplate;
}

public class LanguageService : ILanguageService, IDisposable
{
    private readonly string _basePath;
    private readonly FileSystemWatcher _watcher;
    private readonly ConcurrentDictionary<string, IContextTemplate> _contextTemplates = new();
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _languages = new();
    
    public LanguageService(IEnumerable<IContextTemplate> contextTemplates, string? rootPath = null)
    {
        var baseDir = rootPath ?? AppContext.BaseDirectory;
        _basePath = Path.Combine(baseDir, "Resources", "i18n");
        
        foreach (var contextTemplate in contextTemplates)
        {
            var attribute = contextTemplate.GetType().GetCustomAttribute<LanguageAttribute>();
            if (attribute == null) continue;

            _contextTemplates[attribute.Key.ToLower()] = contextTemplate;
        }
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
        
        _watcher = new FileSystemWatcher
        {
            Path = _basePath,
            Filter = "*.yaml",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        
        _watcher.Changed += OnYamlFileChanged;
        _watcher.Created += OnYamlFileChanged;
        _watcher.Deleted += OnYamlFileChanged;
        _watcher.Renamed += OnYamlFileRenamed;

        LoadAllYamlFiles();
    }
    
    #region Watch Yaml Files
    
    private void OnYamlFileChanged(object sender, FileSystemEventArgs e)
    {
        ReloadYamlFile(e.FullPath);
    }
    
    private void OnYamlFileRenamed(object sender, RenamedEventArgs e)
    {
        ReloadYamlFile(e.FullPath);
    }
    
    private void LoadAllYamlFiles()
    {
        foreach (var file in Directory.EnumerateFiles(_basePath, "*.yaml", SearchOption.AllDirectories))
        {
            ReloadYamlFile(file);
        }
    }
    
    private void ReloadYamlFile(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            _languages.TryRemove(GetRelativeKey(fullPath), out _);
            return;
        }

        try
        {
            var yaml = File.ReadAllText(fullPath);
            var dict = yaml.DeserializeYaml<Dictionary<string, string>>();
            if (dict != null)
                _languages[GetRelativeKey(fullPath)] = dict;
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to load YAML file: {FilePath}. Error: {ErrorMessage}", fullPath, ex.Message);
        }
    }
    
    private string GetRelativeKey(string fullPath)
    {
        var relative = Path.GetRelativePath(_basePath, fullPath);
        var normalized = relative.Replace('\\', '/').ToLower();
        return "/" + normalized.TrimStart('/');
    }
    
    #endregion
    
    #region Init Default Language Yaml
    
    public async Task GenerateDefaultYamlFileAsync(CancellationToken cancellationToken)
    {
        foreach (var ct in _contextTemplates)
        {
            var key = ct.Key.TrimStart('/', '\\');
            
            var relativePath = key.Replace('/', Path.DirectorySeparatorChar);
            
            var filePath = Path.Combine(_basePath, relativePath);
            
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var yamlContent = ct.Value.SerializeYaml();

            await File.WriteAllTextAsync(filePath, yamlContent, cancellationToken);
        }
    }
    
    #endregion

    public string GetContext<T>(Expression<Func<T, object>> expression, LanguageEnum language = LanguageEnum.Default)
        where T : IContextTemplate
    {
        var yamlKey = typeof(T).GetYamlKey(language).ToLower();
        var key = expression.GetPropertyName();

        if (_languages.TryGetValue(yamlKey, out var languageDict) && languageDict.TryGetValue(key, out var context))
            return context;
        
        yamlKey = typeof(T).GetYamlKey().ToLower();
        
        if (!_contextTemplates.TryGetValue(yamlKey, out var contextTemplate))
            return string.Empty;
        
        var propertyInfo = contextTemplate.GetType().GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
        
        if (propertyInfo == null)
            return string.Empty;
        
        var value = propertyInfo.GetValue(contextTemplate)?.ToString();
        
        return !string.IsNullOrEmpty(value) ? value : string.Empty;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}