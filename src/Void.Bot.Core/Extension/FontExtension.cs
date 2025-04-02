using SkiaSharp;

namespace Void.Bot.Core.Extension;

public static class FontExtension
{
    private const string Namespace = "Void.Bot.Core.Assets.Fonts";

    public static Stream GetFontStreamByEmbeddedResource(string resourceName)
    {
        var assembly = typeof(FontExtension).Assembly;
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'.");
        }
        
        return stream;
    }
    
    // 思源黑体 - 汇总
    public static Stream SourceHanSansFontStream => GetFontStreamByEmbeddedResource($"{Namespace}.SourceHanSans.ttc");
    
    // 思源黑体 - 简体中文 - 可变字体
    public static Stream SourceHanSansSCFontStream => GetFontStreamByEmbeddedResource($"{Namespace}.SourceHanSansSC-VF.ttf");
    
    // 思源黑体 - 繁体中文 - 可变字体
    public static Stream SourceHanSansTCFontStream => GetFontStreamByEmbeddedResource($"{Namespace}.SourceHanSansTC-VF.ttf");
}