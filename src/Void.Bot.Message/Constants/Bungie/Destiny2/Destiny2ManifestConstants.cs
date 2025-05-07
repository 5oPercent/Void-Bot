using System.Reflection;
using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.Message.Constants.Bungie.Destiny2;

public partial class Destiny2ManifestConstants
{
    public const string Manifest = nameof(Manifest);
    
    /// <summary>
    /// 组件名称映射，从完整属性名到简化组件名
    /// </summary>
    public static readonly Dictionary<PropertyInfo, string> ComponentPropertyNameMap;

    /// <summary>
    /// 静态构造函数，初始化组件名映射
    /// </summary>
    static Destiny2ManifestConstants()
    {
        var properties = typeof(Destiny2ManifestComponentContentPathDto).GetProperties();
        ComponentPropertyNameMap = properties.ToDictionary(
            p => p,
            p => {
                var name = p.Name;
                if (name.StartsWith("Destiny"))
                    name = name[7..];
                if (name.EndsWith("Definition"))
                    name = name[..^10];
                return name;
            });
    }
}