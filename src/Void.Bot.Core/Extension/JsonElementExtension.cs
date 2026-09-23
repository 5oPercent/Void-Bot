using System.Text.Json;

namespace Void.Bot.Core.Extension;

public static class JsonElementExtension
{
    extension(JsonElement element)
    {
        public uint ReadHash(string fallback)
        {
            var hash = element.ReadUInt32("hash");
            return hash != 0 || !uint.TryParse(fallback, out var parsed) ? hash : parsed;
        }

        public uint ReadUInt32(string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
                return 0;

            if (property.ValueKind == JsonValueKind.Number && property.TryGetUInt32(out var value))
                return value;

            return property.ValueKind == JsonValueKind.String && uint.TryParse(property.GetString(), out value)
                ? value
                : 0;
        }

        public int ReadInt32(string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) &&
                   property.TryGetInt32(out var value)
                ? value
                : 0;
        }

        public bool ReadBoolean(string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) &&
                   property.ValueKind == JsonValueKind.True;
        }

        public string ReadString(string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) &&
                   property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }
    }
}