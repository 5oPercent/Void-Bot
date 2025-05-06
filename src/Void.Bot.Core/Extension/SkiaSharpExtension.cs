using SkiaSharp;

namespace Void.Bot.Core.Extension;

public static class SkiaSharpExtension
{
    public static Stream ToStream(this SKSurface surface, SKEncodedImageFormat imageFormat = SKEncodedImageFormat.Png, int quality = 100)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(imageFormat, quality);
        var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;
        return stream;
    }
    
    /// <summary>
    /// 创建基础画布
    /// </summary>
    public static SKSurface CreateSurface(int width, int height, int margin = 0, SKColor? backgroundColor = null, float cornerRadius = 15f)
    {
        var surface = SKSurface.Create(new SKImageInfo(width + margin, height + margin, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;

        // 清空画布（透明背景）
        canvas.Clear(SKColors.Transparent);
    
        // 计算矩形区域
        using var mainPaint = new SKPaint();
        mainPaint.Color = backgroundColor ?? new SKColor(30, 30, 30, 255);
        mainPaint.IsAntialias = true;

        // 绘制圆角矩形
        using var path = new SKPath();
        path.AddRoundRect(new SKRect(margin, margin, width, height), cornerRadius, cornerRadius);
        canvas.DrawPath(path, mainPaint);
    
        return surface;
    }
}