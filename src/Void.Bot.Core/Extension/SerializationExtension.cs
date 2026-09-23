using System.Text.Json;
using YamlDotNet.Serialization;

namespace Void.Bot.Core.Extension;

public static class SerializationExtension
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string SerializeJson(this object source)
    {
        return JsonSerializer.Serialize(source);
    }
    
    public static T DeserializeJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions)!;
    }

    private static readonly ISerializer YamlSerializer = new SerializerBuilder().Build();
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    public static string SerializeYaml(this object source)
    {
        return YamlSerializer.Serialize(source);
    }
    
    public static T? DeserializeYaml<T>(this string yaml)
    {
        return YamlDeserializer.Deserialize<T>(yaml);
    }
}
